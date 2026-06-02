using CSAnalyzer.Service.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.ServiceModel.Activation;
using System.Net;

namespace CSAnalyzer.Service
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class CSService : ICSService
    {
        private static readonly string SteamApiKey = ConfigurationManager.AppSettings["SteamApiKey"];
        private static readonly string FaceitApiKey = ConfigurationManager.AppSettings["FaceitApiKey"];

        private static readonly HttpClient client = new HttpClient();

        static CSService()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public PlayerProfile GetProfile(string steamId)
        {
            return Task.Run(() => GetProfileAsync(steamId)).Result;
        }

        private async Task<PlayerProfile> GetProfileAsync(string steamId)
        {
            var profile = new PlayerProfile { SteamId = steamId };

            try
            {
                string steamSummaryUrl = $"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={SteamApiKey}&steamids={steamId}";
                var summaryJson = JObject.Parse(await client.GetStringAsync(steamSummaryUrl));
                var player = summaryJson["response"]["players"]?[0];

                if (player == null)
                {
                    profile.Error = "Профиль Steam не найден или скрыт.";
                    return profile;
                }

                profile.Nickname = player["personaname"]?.ToString();
                profile.AvatarUrl = player["avatarfull"]?.ToString();

                if (player["timecreated"] != null)
                {
                    DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds((double)player["timecreated"]);
                    profile.AccountCreatedDate = dt.ToString("MMM dd, yyyy");
                }

                string bansUrl = $"http://api.steampowered.com/ISteamUser/GetPlayerBans/v1/?key={SteamApiKey}&steamids={steamId}";
                var banData = JObject.Parse(await client.GetStringAsync(bansUrl))["players"]?[0];
                if (banData != null)
                {
                    profile.IsVacBanned = (bool)(banData["VACBanned"] ?? false);
                    profile.IsTradeBanned = banData["EconomyBan"]?.ToString() != "none";
                }

                try
                {
                    string gamesUrl = $"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={SteamApiKey}&steamid={steamId}&format=json";
                    var gamesJson = JObject.Parse(await client.GetStringAsync(gamesUrl));
                    var games = gamesJson["response"]["games"];
                    if (games != null)
                    {
                        foreach (var game in games)
                        {
                            if ((int)game["appid"] == 730)
                            {
                                profile.Cs2PlaytimeHours = (int)(game["playtime_forever"] ?? 0) / 60;
                                break;
                            }
                        }
                    }

                    string recentGamesUrl = $"http://api.steampowered.com/IPlayerService/GetRecentlyPlayedGames/v0001/?key={SteamApiKey}&steamid={steamId}";
                    var recentGamesJson = JObject.Parse(await client.GetStringAsync(recentGamesUrl));
                    var recentGames = recentGamesJson["response"]["games"];
                    if (recentGames != null)
                    {
                        foreach (var game in recentGames)
                        {
                            if ((int)game["appid"] == 730)
                            {
                                profile.Cs2RecentPlaytimeHours = (int)(game["playtime_2weeks"] ?? 0) / 60;
                                break;
                            }
                        }
                    }
                }
                catch { /* Ignore if private */ }

                try
                {
                    string levelUrl = $"http://api.steampowered.com/IPlayerService/GetSteamLevel/v1/?key={SteamApiKey}&steamid={steamId}";
                    var levelJson = JObject.Parse(await client.GetStringAsync(levelUrl));
                    profile.SteamLevel = (int?)levelJson["response"]["player_level"];
                }
                catch { }

                try
                {
                    string friendsUrl = $"http://api.steampowered.com/ISteamUser/GetFriendList/v0001/?key={SteamApiKey}&steamid={steamId}&relationship=friend";
                    var friendsResp = await client.GetAsync(friendsUrl);
                    if (friendsResp.IsSuccessStatusCode)
                    {
                        var friendsJson = JObject.Parse(await friendsResp.Content.ReadAsStringAsync());
                        var friendsList = friendsJson["friendslist"]?["friends"] as JArray;
                        if (friendsList != null) profile.FriendsCount = friendsList.Count;
                    }
                }
                catch { /* Ignore if private */ }

                using (var faceitReq = new HttpRequestMessage(HttpMethod.Get, $"https://open.faceit.com/data/v4/players?game=cs2&game_player_id={steamId}"))
                {
                    faceitReq.Headers.Add("Authorization", $"Bearer {FaceitApiKey}");
                    var faceitResp = await client.SendAsync(faceitReq);

                    if (faceitResp.IsSuccessStatusCode)
                    {
                        var faceitJson = JObject.Parse(await faceitResp.Content.ReadAsStringAsync());

                        profile.FaceitNickname = faceitJson["nickname"]?.ToString();
                        profile.FaceitCountry = faceitJson["country"]?.ToString();

                        var cs2Data = faceitJson["games"]?["cs2"];
                        if (cs2Data != null)
                        {
                            profile.FaceitElo = (int?)cs2Data["faceit_elo"];
                        }

                        string playerId = faceitJson["player_id"]?.ToString();
                        if (!string.IsNullOrEmpty(playerId))
                        {
                            using (var statsReq = new HttpRequestMessage(HttpMethod.Get, $"https://open.faceit.com/data/v4/players/{playerId}/stats/cs2"))
                            {
                                statsReq.Headers.Add("Authorization", $"Bearer {FaceitApiKey}");
                                var statsResp = await client.SendAsync(statsReq);

                                if (statsResp.IsSuccessStatusCode)
                                {
                                    var statsJson = JObject.Parse(await statsResp.Content.ReadAsStringAsync());
                                    var lifetime = statsJson["lifetime"];

                                    if (lifetime != null)
                                    {
                                        if (int.TryParse(lifetime["Matches"]?.ToString(), out int matches))
                                            profile.FaceitMatches = matches;

                                        profile.FaceitWinrate = lifetime["Win Rate %"]?.ToString();
                                        profile.FaceitKd = lifetime["Average K/D Ratio"]?.ToString() ?? lifetime["K/D Ratio"]?.ToString();
                                        profile.FaceitHsPercentage = lifetime["Average Headshots %"]?.ToString();

                                        var recent = lifetime["Recent Results"] as JArray;
                                        if (recent != null)
                                        {
                                            profile.FaceitRecentResults = recent.ToObject<string[]>();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                profile.Error = $"Data access error: {ex.Message}";
            }

            return profile;
        }
    }
}