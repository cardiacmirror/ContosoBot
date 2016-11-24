using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using ContosoBot.Models;
using System.Collections.Generic;

namespace ContosoBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                var userMessage = activity.Text;
                HttpClient client = new HttpClient();
                StateClient stateClient = activity.GetStateClient();
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
                Luis.RootObject luisRootObject;
                if (PersistentData.task == "")
                {
                    string b = await client.GetStringAsync(new Uri("https://api.projectoxford.ai/luis/v2.0/apps/48880995-76d4-47f3-80d7-aa856e73ac62?subscription-key=d706e57e79e844f58b5f7b6c1af258ea&q=" + userMessage + "&verbose=true"));
                    luisRootObject = JsonConvert.DeserializeObject<Luis.RootObject>(b);
                }
                if (userMessage.ToLower().Equals("clear") || userMessage.ToLower().Contains("logoff") || luisRootObject.topScoringIntent.intent == "logoff" )
                {
                    await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                    PersistentData.task = "";
                    PersistentData.username = "";
                    PersistentData.usernameLogin = "";
                    Activity infoReply = activity.CreateReply("data cleared");
                    await connector.Conversations.ReplyToActivityAsync(infoReply);
                    return Request.CreateResponse(HttpStatusCode.OK);
                }

                if (userMessage.ToLower().Equals("data"))
                {
                    List<Transactions> transactions = await AzureManager.AzureManagerInstance.GetTransactions();
                    string endOutput = "";
                    foreach (Transactions t in transactions)
                    {
                        endOutput += "[" + t.Date + "] Debit " + t.debit + ", Credit " + t.credit + "\n\n";


                    }
                    Activity infoReply = activity.CreateReply(endOutput);
                    await connector.Conversations.ReplyToActivityAsync(infoReply);
                    return Request.CreateResponse(HttpStatusCode.OK);

                }
                if (PersistentData.task == "")
                {
                    Activity replyToConversation;
                    if (userMessage.ToLower().Contains("stock") || luisRootObject.topScoringIntent.intent == "stock")
                    {
                        PersistentData.task = "stock";
                        replyToConversation = activity.CreateReply($"Please name the company you want  to check");
                        await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                    }
                    else if (userMessage.ToLower().Contains("withdraw") || luisRootObject.topScoringIntent.intent =="withdraw")
                    {
                        if(userData.GetProperty<string>("username")!= null || userData.GetProperty<string>("username") != "")
                        {
                            PersistentData.task = "withdraw";
                            replyToConversation = activity.CreateReply($"How much do you want to withdraw");
                            await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                        }
                        else
                        {
                            replyToConversation = activity.CreateReply($"You need to be logged in to perform this task");
                            await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                        }

                    }
                    else if (userMessage.ToLower().Contains("deposit") || luisRootObject.topScoringIntent.intent =="deposit")
                    {
                        if (userData.GetProperty<string>("username") != null || userData.GetProperty<string>("username") != "")
                        {
                            PersistentData.task = "deposit";
                            replyToConversation = activity.CreateReply($"How much do you want to deposit");
                            await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                        }
                        else
                        {
                            replyToConversation = activity.CreateReply($"You need to be logged in to perform this task");
                            await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                        }
                    }

                    else if (userMessage.ToLower().Contains("view transactions") || luisRootObject.topScoringIntent.intent =="view transactions")
                    {

                        if (userData.GetProperty<string>("username") != null && userData.GetProperty<string>("username") != "")
                        {
                            List<Transactions> transactions = await AzureManager.AzureManagerInstance.GetUserTransactions(userData.GetProperty<string>("username"));
                            string endOutput = "";
                            foreach (Transactions t in transactions)
                            {
                                endOutput += "[" + t.Date + "] Debit " + t.debit + ", Credit " + t.credit + "\n\n";


                            }
                            List<Users> user = await AzureManager.AzureManagerInstance.GetSpecificUser(userData.GetProperty<string>("username"));
                            endOutput += "Final Balancec " + user[0].balance;
                            Activity infoReply = activity.CreateReply(endOutput);
                            await connector.Conversations.ReplyToActivityAsync(infoReply);
                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        else
                        {
                            Activity infoReply = activity.CreateReply("you need to logged in to perform this task");
                            await connector.Conversations.ReplyToActivityAsync(infoReply);
                            return Request.CreateResponse(HttpStatusCode.OK);
                        }


                    }
                    else if (userMessage.ToLower()=="create user" || luisRootObject.topScoringIntent.intent =="create user")
                    {
                        if (userData.GetProperty<string>("username") == null || userData.GetProperty<string>("username") == "")
                        {
                            PersistentData.task = "user creation";
                            replyToConversation = activity.CreateReply($"Please enter a username");
                            await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                        }
                        else
                        {
                            replyToConversation = activity.CreateReply($"You can not create a user when you are logge din");
                            await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                        }
                    }
                    else if (userMessage.ToLower().Contains("login") || luisRootObject.topScoringIntent.intent =="login")
                    {
                        if(userData.GetProperty<string>("username") == null || userData.GetProperty<string>("username") == "")
                        {
                            PersistentData.task = "login";
                            replyToConversation = activity.CreateReply($"Please provide me a username");
                            await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                        }
                        else
                        {
                            replyToConversation = activity.CreateReply($"Please logoff before you try to logine");
                            await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                        }
                    }
                    else if (userMessage.ToLower().Contains("clear my transaction") || luisRootObject.topScoringIntent.intent == "clear transactions")
                    {
                        if (userData.GetProperty<string>("username") != null && userData.GetProperty<string>("username") != "")
                        {
                            PersistentData.task = "clear transactions";
                            replyToConversation = activity.CreateReply($"Please enter your password to confirm");
                            await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                        }
                        else
                        {
                            replyToConversation = activity.CreateReply($"You need to be logged in to perform this task");
                            await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                        }
                    }


                        return Request.CreateResponse(HttpStatusCode.OK);
                }


                else
                {
                    if (PersistentData.task == "stock")
                    {


                        Activity replyToConversation;
                        StockQuote.RootObject stockRootObject;
                        string a = await client.GetStringAsync(new Uri("http://dev.markitondemand.com/Api/v2/Quote/json?symbol=" + userMessage));
                        stockRootObject = JsonConvert.DeserializeObject<StockQuote.RootObject>(a);
                        if (stockRootObject.Status != "SUCCESS")
                        {
                            replyToConversation = activity.CreateReply($"Could not find this company");
                            await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                            PersistentData.task = "";
                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        replyToConversation = activity.CreateReply($"Stock Quote of");
                        replyToConversation.Attachments = new List<Attachment>();
                        List<CardImage> cardImages = new List<CardImage>();
                        cardImages.Add(new CardImage("https://cdn2.iconfinder.com/data/icons/miscellaneous-31/60/presentation-arrow-128.png"));
                        ThumbnailCard plCard = new ThumbnailCard()
                        {
                            Title = stockRootObject.Name ,
                            Subtitle = "Last Price $" + stockRootObject.LastPrice + " USD, Change Percentage " + Math.Round(stockRootObject.ChangePercent,2) + "% \n\n " + "Timestamp " + stockRootObject.Timestamp,
                            Images = cardImages
                        };
                        Attachment plAttachment = plCard.ToAttachment();
                        replyToConversation.Attachments.Add(plAttachment);
                        await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                        PersistentData.task = "";
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }

                    else if (PersistentData.task == "withdraw")
                    {
                        Activity replyToConversation;
                        if (!userMessage.All(Char.IsDigit))
                        {
                            replyToConversation = activity.CreateReply($"Invalid Characters");
                            await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                            PersistentData.task = "";
                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        double amount = Convert.ToDouble(userMessage);
                        Transactions withdraw = new Transactions()
                        {
                            credit = amount,
                            username = userData.GetProperty<string>("username")
                        };
                        await AzureManager.AzureManagerInstance.AddTransaction(withdraw);
                        List<Users> user = await AzureManager.AzureManagerInstance.GetSpecificUser(userData.GetProperty<string>("username"));
                        user[0].balance -= amount;
                        await AzureManager.AzureManagerInstance.UpdateUser(user[0]);
                        PersistentData.task = "";
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else if (PersistentData.task == "deposit")
                    {
                        Activity replyToConversation;
                        if (!userMessage.All(Char.IsDigit))
                        {
                            replyToConversation = activity.CreateReply($"Invalid Characters");
                            await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                            PersistentData.task = "";
                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        double amount = Convert.ToDouble(userMessage);
                        Transactions deposit = new Transactions()
                        {
                            debit = amount,
                            username = userData.GetProperty<string>("username")
                        };
                        await AzureManager.AzureManagerInstance.AddTransaction(deposit);
                        List<Users> user = await AzureManager.AzureManagerInstance.GetSpecificUser(userData.GetProperty<string>("username"));
                        user[0].balance += amount;
                        await AzureManager.AzureManagerInstance.UpdateUser(user[0]);
                        PersistentData.task = "";
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else if (PersistentData.task == "user creation")
                    {
                        Activity replyToConversation;
                        if (PersistentData.username == "")
                        {
                            List<Users> users = await AzureManager.AzureManagerInstance.GetUsers();
                            foreach (Users t in users)
                            {
                                if (t.username == userMessage)
                                {
                                    replyToConversation = activity.CreateReply($"the username is taken please give me anthoer one");
                                    await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                            }
                            PersistentData.username = userMessage;
                            replyToConversation = activity.CreateReply($"Please give e the password for this account");
                            await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                            return Request.CreateResponse(HttpStatusCode.OK);

                        }
                        else
                        {
                            Users user = new Users()
                            {
                                username = PersistentData.username,
                                password = userMessage
                            };
                            await AzureManager.AzureManagerInstance.AddUser(user);
                            replyToConversation = activity.CreateReply($"You have sucefully made an acounnt and logged in");
                            userData.SetProperty<string>("username", PersistentData.username);
                            PersistentData.username = "";
                            PersistentData.task = "";
                            await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                    }
                    else if (PersistentData.task == "clear transactions")
                    {
                        Activity replyToConversation;
                        List<Users> user = await AzureManager.AzureManagerInstance.GetSpecificUser(userData.GetProperty<string>("username"));
                        if (user[0].password == userMessage)
                        {
                            List<Transactions> transactions = await AzureManager.AzureManagerInstance.GetUserTransactions(userData.GetProperty<string>("username"));
                            foreach (Transactions t in transactions)
                            {
                                await AzureManager.AzureManagerInstance.DeleteTransaction(t);

                            }
                            replyToConversation = activity.CreateReply($"your transactions have been cleared");
                            await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                            PersistentData.task = "";
                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        else
                        {
                            replyToConversation = activity.CreateReply($"your password was incorrect");
                            await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                            PersistentData.task = "";
                            return Request.CreateResponse(HttpStatusCode.OK);
                        }

                    }
                    else if (PersistentData.task == "login")
                    {
                        Activity replyToConversation;
                        if (PersistentData.usernameLogin == "")
                        {
                            PersistentData.usernameLogin = userMessage;
                            replyToConversation = activity.CreateReply($"Please enter the password");
                            await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        else
                        {
                            List<Users> user = await AzureManager.AzureManagerInstance.GetSpecificUser(PersistentData.usernameLogin);
                            if(user.Count == 0)
                            {
                                PersistentData.usernameLogin = "";
                                replyToConversation = activity.CreateReply($"One of the credientials you entered was incorrect");
                                await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                                PersistentData.task = "";
                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else if (user[0].password != userMessage)
                            {
                                replyToConversation = activity.CreateReply($"One of the credientials you entered was incorrect");
                                await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                                PersistentData.task = "";
                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                userData.SetProperty<string>("username", PersistentData.usernameLogin);
                                await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                                PersistentData.usernameLogin = "";
                                PersistentData.task = "";
                                replyToConversation = activity.CreateReply($"Sucefully logged in");
                                await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                        }
                    }
                }

                Activity reply = activity.CreateReply($"Did not understand command");
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}

public static class PersistentData
{
    public static string task= "";
    public static string username = "";
    public static string usernameLogin = "";
}