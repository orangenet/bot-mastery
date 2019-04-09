using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace MsGraphBot
{
    public class CalendarClient
    {
        private readonly GraphServiceClient _client;

        private CalendarClient(GraphServiceClient client)
        {
            _client = client;
        }

        public static CalendarClient FromToken(string token)
        {
            return new CalendarClient(new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    requestMessage =>
                    {
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
                        requestMessage.Headers.Add("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"");

                        return Task.CompletedTask;
                    })));
        }
        
        public async Task<IEnumerable<Event>> GetCalendarEvents()
        {
            List<Option> options = new List<Option>();

            options.Add(new QueryOption("startDateTime", DateTime.Now.ToString("o").Split("+")[0]));
            options.Add(new QueryOption("endDateTime", DateTime.Now.AddDays(7).ToString("o").Split("+")[0]));
            options.Add(new HeaderOption("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\""));

            var calendarView = await _client.Me.Calendar.CalendarView.Request(options).GetAsync();

            return calendarView.CurrentPage;
        }
    }
}