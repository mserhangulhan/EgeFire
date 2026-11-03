using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EasyTaskStarter.Services
{
    public static class UpdateChecker
    {
        private const string RepoOwner = "mserhangulhan";
        private const string RepoName = "EgeFire";
        private const string ApiUrl = "https://api.github.com/repos/" + RepoOwner + "/" + RepoName + "/releases/latest";

        public static async Task<(bool HasUpdate, string LatestVersion, string ReleaseNotes, string DownloadUrl)> CheckForUpdatesAsync(string currentVersion)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("EgeFire-Updater");
                client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github.v3+json");

                var response = await client.GetStringAsync(ApiUrl);
                var json = JObject.Parse(response);

                var tagName = json["tag_name"]?.ToString()?.TrimStart('v', 'V') ?? "";
                var body = json["body"]?.ToString() ?? "";
                var assets = json["assets"] as JArray;
                string downloadUrl = "";

                if (assets != null)
                {
                    foreach (var asset in assets)
                    {
                        var name = asset["name"]?.ToString()?.ToLower() ?? "";
                        if (name.EndsWith(".exe") || name.EndsWith(".msix") || name.EndsWith(".zip"))
                        {
                            downloadUrl = asset["browser_download_url"]?.ToString() ?? "";
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(tagName))
                    return (false, currentVersion, "", "");

                var hasUpdate = IsNewerVersion(tagName, currentVersion);
                return (hasUpdate, tagName, body, downloadUrl);
            }
            catch
            {
                return (false, currentVersion, "", "");
            }
        }

        private static bool IsNewerVersion(string latest, string current)
        {
            try
            {
                var latestParts = latest.Split('.');
                var currentParts = current.Split('.');

                for (int i = 0; i < Math.Max(latestParts.Length, currentParts.Length); i++)
                {
                    var l = i < latestParts.Length ? int.Parse(latestParts[i]) : 0;
                    var c = i < currentParts.Length ? int.Parse(currentParts[i]) : 0;
                    if (l > c) return true;
                    if (l < c) return false;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}