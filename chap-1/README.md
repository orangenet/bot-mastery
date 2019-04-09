# Hello World Bot

Let's get started with the most simplistic bot that we can build. You guessed it a "Hello World" bot :)

# Getting Started
To short-circuit the interaction with a bot you need to install the BotFramework emulator. 
1. [Download the Bot Framework Emulator](https://github.com/Microsoft/BotFramework-Emulator/releases)

When the bot is not hosted locally, and you want to test with the Emulator (which is running on your computer) the bot won't be able to communicate back to it. To get a publicly accessible IP and tunnel the requests back to the Emulator we need ngrok.

2. Install [ngrok](https://ngrok.com/)

Question: Do I need Visual Studio, dotnet 2.1, and and and...? No! We designed this repository to work with Gitpod.io. But if you want to develop locally install those apps and clone the repo. For the others let's get Gitpod ready!

### Gitpod
#### What is Gitpod?
* Gitpod is an online IDE, that enables you to have the whole environment prepared, with access to Bash or w.e. you choose.
* Starting applications on the online IDE will create a publicly accessible IP so that you can test your application.

#### Getting started with Gitpod
1. Login with your Github account: https://gitpod.io/api/login
2. Click the button below!

[![Open in Gitpod](https://gitpod.io/button/open-in-gitpod.svg)](https://gitpod.io/#https://github.com/orangenet/bot-mastery)

#### Running the first app
1. Run the application by executing the command below:
```
cd ./chap-1/Hello && \
    dotnet run
```
2. Click on `Open Browser` as shown below and copy the URL:
![Gitpod](./images/gitpod-generated-url.png)

3. Open the emulator and create a Bot as shown below, using the URL in step 2 as the Endpoint URL:
![Configure bot emulator](./images/configure-bot-in-emulator.png)

Start chatting with the bot!
![chatting](./images/chatting.png)
