using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Buider.Luis.Models;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;


// For more information about this template visit http://aka.ms/azurebots-csharp-basic
[Serializable]
public class EchoDialog : IDialog<object>
{
    protected int count = 1;

    public Task StartAsync(IDialogContext context)
    {
        try
        {
            context.Wait(MessageReceivedAsync);
        }
        catch (OperationCanceledException error)
        {
            return Task.FromCanceled(error.CancellationToken);
        }
        catch (Exception error)
        {
            return Task.FromException(error);
        }

        return Task.CompletedTask;
    }

    public class EchoDialog : LuisDialog<object>

        private const string RideSharingApiUrl = "https://ready-ill-api.azurewebsites.net/api/ride-sharing/price";

    
    public EchoDialog()
        :base(new LuisService(new LuisModelAttribute(Utils.GetAppSetting("33f0f7f1-9880-4b20-8133-68df0f26a8d8"), 
            Utils.GetAppSetting("83b82f336e8c4be5835f2bece36be490"))))
    {

    }

    [LuisIntent("Greetings")]
    public aync Task Greetings(IDialogContext context, LuisResult result)
    {
        await context.PostAsync("Hello! I am your ride sharing assistant.");
        context.Wait(MessageReceived);
    }

    [LuisIntent("GetPriceEstimate")]
    public async Task GetPriceEstimate(IDialogContext context, LuisResult result)
    {
        EntityRecommendation location;
        if (result.TryFindEntity("location", out location))
        {
            string rs;

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"{RideSharingApiUrl}?from=Las Vegas&to={location.Entity}");
                response.EnsureSuccessStatusCode();
                rs = await response.Content.ReadAsAsync<string>();
            }

            await context.PostAsync($"I estimate that it will cost {rs} to get to {location.Entity}.");
         }
        context.Wait(MessageReceived);
    }

    [LuisIntent("GetKindOfVehicles")]
    public async Task GetKindOfVehicles(IDialogContext context, LuisResult result)
    {
        //Initialize Carousel
        var replyToConversation = context.MakeMessage();
        replyToConversation.AttachmentLayout = AttachmentLayoutTypes.Carousel;
        replyToConversation.Type = ActivityTypes.Message;

        // Add card to carousel    
        var carTypes = new string[] { "Sedan", "SUV", "Sports car" };
        var actions = new List<CardAction>();
        foreach (string type in carTypes)
        {
            CardAction kind = new CardAction
            {
                Value = type,
                Type = "imBack",
                Title = type
            };
            actions.Add(kind);
        }

        // Add card with options to the Carousel    
        HeroCard card = new HeroCard
        {
            Title = "Kinds of cars available",
            Buttons = actions
        };

        replyToConversation.Attachments = new List<Attachment> { card.ToAttachment() };
        await context.PostAsync(replyToConversation);
        context.Wait(MessageReceivedAsync);
    }

    [LuisIntent("None")]
    [LuisIntent("")]
    public async Task None(IDialogContext context, 
        IAwaitable<ImessageActivity> message, LuisResult result)
{
var messageToForward = await message;
await context.Forward(new QnADialog(); AfterQnADialog, messageToForward, CancellationToken.None);
}

//This method will handle all inquiries that aren't recognized by LUIS and route them to QnA Maker.
private async Task AfterQnADialog(IDialogContext context,
IAwaitable<object> result)
{
context.Wait(MessageReceived);
}


    public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
    {
        var message = await argument;
        if (message.Text == "reset")
        {
            PromptDialog.Confirm(
                context,
                AfterResetAsync,
                "Are you sure you want to reset the count?",
                "Didn't get that!",
                promptStyle: PromptStyle.Auto);
        }
        else
        {
            await context.PostAsync($"{this.count++}: You said {message.Text}");
            context.Wait(MessageReceivedAsync);
        }
    }

    public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;
        if (confirm)
        {
            this.count = 1;
            await context.PostAsync("Reset count.");
        }
        else
        {
            await context.PostAsync("Did not reset count.");
        }
        context.Wait(MessageReceivedAsync);
    }
}

//towards the end

[Serializable]
public class QnADialog : QnAMakerDialog
{
    public QnADialog()
        :base(new QnAMakerService(new QnAMakerAttribute(Utils.GetAppSetting ("QnASubscriptionKey"),
        Utils.GetAppSetting("QnAKnowledgebaseId"), "Sorry but I can't find the answer. Could you rephrase your question?", 0.1)))
    {
    }
}