// <copyright file="MainDialog.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace BFSSO.Dialogs
{
    /// <summary>
    /// Main dialog that handles the authentication and user interactions.
    /// </summary>
    public class MainDialog : LogoutDialog
    {
        protected readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainDialog"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        public MainDialog(IConfiguration configuration, ILogger<MainDialog> logger)
            : base(nameof(MainDialog), configuration["ConnectionName"])
        {
            _logger = logger;

            AddDialog(new OAuthPrompt(
                "graphOAuthPrompt",
                new OAuthPromptSettings
                {
                    ConnectionName = configuration["ConnectionName"],
                    Text = "Please Sign In",
                    Title = "Sign In in" + configuration["ConnectionName"],
                    Timeout = 300000, // User has 5 minutes to login (1000 * 60 * 5)
                    EndOnInvalidMessage = true
                }));

            AddDialog(new OAuthPrompt(
                "ghOAuthPrompt",
                new OAuthPromptSettings
                {
                    ConnectionName = "gh",
                    Text = "Please Sign In",
                    Title = "Sign In GH",
                    Timeout = 300000, // User has 5 minutes to login (1000 * 60 * 5)
                    EndOnInvalidMessage = true
                }));

            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                    PromptGraphStepAsync,
                    HandleGraphTokenAsync,
                    PromptForGHTokenAsync,
                    HandleGHTokenAsync,
                    FinalStep,
                    DisplayTokenPhase1Async,
                    DisplayTokenPhase2Async
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        /// <summary>
        /// Prompts the user to sign in.
        /// </summary>
        /// <param name="stepContext">The waterfall step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task<DialogTurnResult> PromptGraphStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("PromptStepAsync() called.");
            return await stepContext.BeginDialogAsync("graphOAuthPrompt", null, cancellationToken);
        }

        private async Task<DialogTurnResult> HandleGraphTokenAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse != null)
            {
                // Store the Graph token for later use
                stepContext.Values["ssoToken"] = tokenResponse.Token;
            }
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> PromptForGHTokenAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync("ghOAuthPrompt", null, cancellationToken);
        }

        private async Task<DialogTurnResult> HandleGHTokenAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse != null)
            {
                stepContext.Values["ghToken"] = tokenResponse.Token;
            }
            return await stepContext.NextAsync(null, cancellationToken);
        }

        /// <summary>
        /// Handles the login step.
        /// </summary>
        /// <param name="stepContext">The waterfall step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task<DialogTurnResult> FinalStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var oauthValues = stepContext.Values;
            if (oauthValues != null)
            {
                try
                {

                    if (oauthValues.ContainsKey("ssoToken"))
                    {
                        var client = new SimpleGraphClient(oauthValues["ssoToken"].ToString()!);
                        var me = await client.GetMeAsync();
                        var title = !string.IsNullOrEmpty(me.JobTitle) ? me.JobTitle : "Unknown";

                        await stepContext.Context.SendActivityAsync($"You're logged in as {me.DisplayName} ({me.UserPrincipalName}); your job title is: {title}");

                        //var photo = await client.GetPhotoAsync();

                        //if (!string.IsNullOrEmpty(photo))
                        //{
                        //    var cardImage = new CardImage(photo);
                        //    var card = new ThumbnailCard(images: new List<CardImage> { cardImage });
                        //    var reply = MessageFactory.Attachment(card.ToAttachment());

                        //    await stepContext.Context.SendActivityAsync(reply, cancellationToken);
                        //}
                        //else
                        //{
                        //    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Sorry! User doesn't have a profile picture to display."), cancellationToken);
                        //}
                    }

                    if (oauthValues.ContainsKey("ghToken"))
                    {
                        var githubClient = new GitHubSimpleClient(oauthValues["ghToken"].ToString()!);
                        var user = await githubClient.GetUserInfoAsync();
                        await stepContext.Context.SendActivityAsync($"You're logged in GH as {user})", cancellationToken: cancellationToken);
                    }

                    return await stepContext.PromptAsync(
                        nameof(ConfirmPrompt),
                        new PromptOptions { Prompt = MessageFactory.Text("Would you like to view your token?") },
                        cancellationToken);

                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occurred while processing your request.", ex);
                }
            }
            else
            {
                _logger.LogInformation("Response token is null or empty.");
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login was not successful, please try again."), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Displays the token if the user confirms.
        /// </summary>
        /// <param name="stepContext">The waterfall step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task<DialogTurnResult> DisplayTokenPhase1Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("DisplayTokenPhase1Async() method called.");

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you."), cancellationToken);

            var result = (bool)stepContext.Result;
            if (result)
            {
                return await stepContext.BeginDialogAsync("graphOAuthPrompt", cancellationToken: cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Displays the token to the user.
        /// </summary>
        /// <param name="stepContext">The waterfall step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task<DialogTurnResult> DisplayTokenPhase2Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("DisplayTokenPhase2Async() method called.");

            //var tokenResponse = (TokenResponse)stepContext.Result;
            //if (tokenResponse != null)
            //{
            //    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Here is your token: {tokenResponse.Token}"), cancellationToken);
            //}

            var oauthValues = stepContext.Values;
            if (oauthValues.ContainsKey("ghToken"))
            {
                var githubClient = new GitHubSimpleClient(oauthValues["ghToken"].ToString()!);
                var user = await githubClient.GetUserInfoAsync();
                await stepContext.Context.SendActivityAsync($"You're logged in GH as {user})", cancellationToken: cancellationToken);
            }

            if (oauthValues.ContainsKey("ssoToken"))
            {
                var client = new SimpleGraphClient(oauthValues["ssoToken"].ToString()!);
                var me = await client.GetMeAsync();
                var title = !string.IsNullOrEmpty(me.JobTitle) ? me.JobTitle : "Unknown";

                await stepContext.Context.SendActivityAsync($"You're logged in Entra as {me.DisplayName} ({me.UserPrincipalName}); your job title is: {title}");
            }

                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}