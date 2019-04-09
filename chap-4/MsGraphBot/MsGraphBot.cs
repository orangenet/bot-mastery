using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace MsGraphBot
{
    public class MsGraphBot : IBot
    {
        private const string ConnectionName = "BotAuthentication";

        private const string LoginPromptName = "loginPrompt";
        private const string ConfirmPromptName = "confirmPrompt";
        private const string WaterfallDialogName = "waterfallDialog";

        private const string WelcomeText = @"This bot will introduce you to Authentication.
                                        Type anything to get logged in. Type 'logout' to sign-out.
                                        Type 'help' to view this message again";

        private readonly DialogSet _dialogs;

        private readonly OAuthPromptSettings _oauthPromptSettings = new OAuthPromptSettings
        {
            ConnectionName = ConnectionName, // The connection name used in New Connection Setting
            Text = "Please Sign In",
            Title = "Sign In",
            Timeout = 300000, // Time to log in
        };

        private readonly ConversationState _conversationState;
        private readonly IStatePropertyAccessor<DialogState> _dialogStateAccessor;

        public MsGraphBot(ConversationState conversationState)
        {
            _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            _dialogStateAccessor = _conversationState.CreateProperty<DialogState>(nameof(DialogState));
            _dialogs = new DialogSet(_dialogStateAccessor);

            _dialogs.Add(new OAuthPrompt(LoginPromptName, _oauthPromptSettings));
            _dialogs.Add(new ConfirmPrompt(ConfirmPromptName));
            _dialogs.Add(new WaterfallDialog(WaterfallDialogName,
                new WaterfallStep[] {PromptStepAsync, LoginStepAsync, DisplayTokenAsync}));
        }

        public async Task OnTurnAsync(ITurnContext turnContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var dc = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.Message:

                    var text = turnContext.Activity.Text.ToLowerInvariant();
                    if (text == "help")
                    {
                        await turnContext.SendActivityAsync(WelcomeText, cancellationToken: cancellationToken);
                        break;
                    }

                    if (text == "logout")
                    {
                        var botAdapter = (BotFrameworkAdapter) turnContext.Adapter;
                        await botAdapter.SignOutUserAsync(turnContext, ConnectionName,
                            cancellationToken: cancellationToken);
                        await turnContext.SendActivityAsync("You have been signed out.",
                            cancellationToken: cancellationToken);
                        await turnContext.SendActivityAsync(WelcomeText, cancellationToken: cancellationToken);
                        break;
                    }

                    await dc.ContinueDialogAsync(cancellationToken);

                    if (!turnContext.Responded)
                    {
                        await dc.BeginDialogAsync(WaterfallDialogName, cancellationToken: cancellationToken);
                    }

                    break;
                case ActivityTypes.Event:
                case ActivityTypes.Invoke:
                    dc = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                    await dc.ContinueDialogAsync(cancellationToken);
                    if (!turnContext.Responded)
                    {
                        await dc.BeginDialogAsync(WaterfallDialogName, cancellationToken: cancellationToken);
                    }

                    break;
                case ActivityTypes.ConversationUpdate:
                    if (turnContext.Activity.MembersAdded != null)
                    {
                        await SendWelcomeMessageAsync(turnContext, cancellationToken);
                    }

                    break;
            }

            await _conversationState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);
        }

        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(
                        $"Welcome to AuthenticationBot {member.Name}. {WelcomeText}",
                        cancellationToken: cancellationToken);
                }
            }
        }

        private static async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext step,
            CancellationToken cancellationToken)
        {
            return await step.BeginDialogAsync(LoginPromptName, cancellationToken: cancellationToken);
        }

        private static async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext step,
            CancellationToken cancellationToken)
        {
            var tokenResponse = (TokenResponse) step.Result;
            if (tokenResponse != null)
            {
                await step.Context.SendActivityAsync("You are now logged in.", cancellationToken: cancellationToken);
                return await step.PromptAsync(
                    ConfirmPromptName,
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Would you like to view your calendar entries?")
                    },
                    cancellationToken);
            }

            await step.Context.SendActivityAsync("Login was not successful please try again.",
                cancellationToken: cancellationToken);
            return Dialog.EndOfTurn;
        }

        private static async Task<DialogTurnResult> DisplayTokenAsync(WaterfallStepContext step,
            CancellationToken cancellationToken)
        {
            var result = (bool) step.Result;
            if (result)
            {
                var prompt = await step.BeginDialogAsync(LoginPromptName, cancellationToken: cancellationToken);
                var tokenResponse = (TokenResponse) prompt.Result;

                if (tokenResponse != null)
                {
                    var events = await CalendarClient.FromToken(tokenResponse.Token).GetCalendarEvents();
                    string eventsFormatted = string.Join("\n",
                        events.Select(s => $"- {s.Subject} at {DateTime.Parse(s.Start.DateTime).ToShortTimeString()} ")
                            .ToList());
                    
                    await step.Context.SendActivityAsync("You have the following events: \n" + eventsFormatted,
                        cancellationToken: cancellationToken);
                }
            }

            return Dialog.EndOfTurn;
        }
    }
}