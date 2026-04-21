using Octokit;

namespace SparkSSO
{
    public class GitHubSimpleClient(string token)
    {
        public async Task<string> GetUserInfoAsync()
        {
            var github = new GitHubClient(new ProductHeaderValue("GitHubOAuthDemo"))
            {
                Credentials = new Credentials(token)
            };

            var user = await github.User.Current();

            return $"{user.Name} {user.Email} public repos: {user.PublicRepos}";
        }
    }
}
