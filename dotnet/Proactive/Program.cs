// See https://aka.ms/new-console-template for more information
using Microsoft.AspNetCore.Builder;
using Microsoft.Teams.Api;
using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Api.Messages;
using Microsoft.Teams.Plugins.AspNetCore.Extensions;

Console.WriteLine("Hello, World!");

var builder = WebApplication.CreateBuilder(args);
builder.AddTeams();
var webApp = builder.Build();

var teamsApp = webApp.UseTeams();
await teamsApp.Start();

string cid = "19:9f2af1bee7cc4a71af25ac72478fd5c6";
string userId = "28:56653e9d-2158-46ee-90d7-675c39642038";
string ServiceUrl = "https://smba.trafficmanager.net/teams/";
var targetedMessage = new MessageActivity("Hey! This is a private message just for you!")
{
    Recipient = new Account { Id = userId }
};

await teamsApp.Send(cid, targetedMessage, ConversationType.Channel, serviceUrl: ServiceUrl, isTargeted: true);