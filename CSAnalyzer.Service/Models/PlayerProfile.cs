using System.Runtime.Serialization;

namespace CSAnalyzer.Service.Models
{
    [DataContract]
    public class PlayerProfile
    {
        // Steam Basic
        [DataMember(Name = "steamId")] public string SteamId { get; set; }
        [DataMember(Name = "nickname")] public string Nickname { get; set; }
        [DataMember(Name = "avatarUrl")] public string AvatarUrl { get; set; }
        [DataMember(Name = "isVacBanned")] public bool IsVacBanned { get; set; }
        [DataMember(Name = "isTradeBanned")] public bool IsTradeBanned { get; set; }

        // Steam Extended
        [DataMember(Name = "accountCreatedDate")] public string AccountCreatedDate { get; set; }
        [DataMember(Name = "steamLevel")] public int? SteamLevel { get; set; }
        [DataMember(Name = "friendsCount")] public int? FriendsCount { get; set; }
        [DataMember(Name = "cs2PlaytimeHours")] public int Cs2PlaytimeHours { get; set; }
        [DataMember(Name = "cs2RecentPlaytimeHours")] public int Cs2RecentPlaytimeHours { get; set; }

        // Faceit Basic
        [DataMember(Name = "faceitNickname")] public string FaceitNickname { get; set; }
        [DataMember(Name = "faceitCountry")] public string FaceitCountry { get; set; }
        [DataMember(Name = "faceitElo")] public int? FaceitElo { get; set; }

        // Faceit Stats
        [DataMember(Name = "faceitMatches")] public int? FaceitMatches { get; set; }
        [DataMember(Name = "faceitWinrate")] public string FaceitWinrate { get; set; }
        [DataMember(Name = "faceitKd")] public string FaceitKd { get; set; }
        [DataMember(Name = "faceitHsPercentage")] public string FaceitHsPercentage { get; set; }
        [DataMember(Name = "faceitRecentResults")] public string[] FaceitRecentResults { get; set; }

        [DataMember(Name = "error")] public string Error { get; set; }
    }
}