using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Handlers;
using Microsoft.Teams.Apps.Schema;
using Polly;

var webAppBuilder = WebApplication.CreateSlimBuilder(args);
webAppBuilder.AddTeams();
var webApp = webAppBuilder.Build();
var teamsApp = webApp.UseTeams();

//var mentionAccount = new TeamsConversationAccount
//{
//    Id = "29:18KsxwFhEzeddx4SMqP-oXmrXC1ahSR5xCWhycCEpSq5er61p_VbYrGLCLMSGFno_ISvxcGhDDPvm0FaY-r0B-A",
//    Name = "msdocsaibot"
//};

//var mentionAccount = new TeamsConversationAccount
//{
//    Id = "29:114JPWms8v7Csvt6kqzXOfRl59tlI0HqYy3P1ZdrvkX7SSt7v9nK1SULFCsKhJky8HlqeUeaPIYP8G1W-W0Sr5Q",
//    Name = "Rido",
//    AadObjectId = "03500558-e554-416c-90c3-a061cdcd012b"
//};

//var mentionAccount = new TeamsConversationAccount
//{
//    Id = "8:orgid:6037c803-c89a-4788-afae-8c9d24836a6e",
//    Name = "TestCoreAgent01",
//};


teamsApp.OnMessage(async (ctx, ct) =>
{
    var moreReply = TeamsActivity.CreateBuilder()
        .WithText("Tell me more about the second option")
        .Build();
    await ctx.SendActivityAsync(moreReply, ct);

    var newMsg = TeamsActivity.CreateBuilder()
        .WithServiceUrl(ctx.Activity.ServiceUrl)
        .WithConversation(new TeamsConversation { Id = "19:03500558-e554-416c-90c3-a061cdcd012b_8c4e81cc-64a0-47fd-bfd1-76ad8dca9c60@unq.gbl.spaces" })
        .WithText("What is the max for a double in C#?")
        .Build();

    await teamsApp.SendActivityAsync(newMsg, ct);
});


teamsApp.OnMessage("--diag", async (ctx, ct) => {

    string diagInfo = $"version `v: {TeamsBotApplication.Version}` \r\n" +
                      $"cid `{ctx.Activity.Conversation?.Id}` \r\n" +
                      $"aid `{ctx.Activity.Id}` \r\n" +
                      $"from `{ctx.Activity.From?.Id}` \r\n";

    var diagMsg = TeamsActivity.CreateBuilder()
        .WithText(diagInfo, TextFormats.Markdown)
        .Build();

    await ctx.SendActivityAsync(diagMsg);
});

webApp.Run();