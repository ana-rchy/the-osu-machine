using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

public class Commands : BaseCommandModule {
    class ScoreInfo {
        [JsonPropertyName("mods")] public string[] Mods { get; set; }
        [JsonPropertyName("accuracy")] public float Accuracy { get; set; }
        [JsonPropertyName("rank")] public string Rank { get; set; }
        [JsonPropertyName("pp")] public float? PP { get; set; }
        [JsonPropertyName("max_combo")] public int Combo { get; set; }
        [JsonPropertyName("score")] public long Score { get; set; }

        [JsonPropertyName("statistics")] public HitStats HitStats { get; set; }
        [JsonPropertyName("beatmap")] public BeatmapInfo Beatmap { get; set; }
        [JsonPropertyName("beatmapset")] public BeatmapSetInfo BeatmapSet { get; set; }
    }

    class HitStats {
        [JsonPropertyName("count_300")] public int Count300 { get; set; }
        [JsonPropertyName("count_100")] public int Count100 { get; set; }
        [JsonPropertyName("count_50")] public int Count50 { get; set; }
        [JsonPropertyName("count_miss")] public int CountMiss { get; set; }
    }

    class BeatmapInfo {
        [JsonPropertyName("id")] public int ID { get; set; }
        [JsonPropertyName("version")] public string Diff { get; set; }
        [JsonPropertyName("difficulty_rating")] public float BaseSR { get; set; }
        [JsonPropertyName("count_circles")] public int CountCircles { get; set; }
        [JsonPropertyName("count_sliders")] public int CountSliders { get; set; }
        [JsonPropertyName("count_spinners")] public int CountSpinners { get; set; }
    }

    class BeatmapExtendedInfo {
        [JsonPropertyName("max_combo")] public int MaxCombo { get; set; }
    }

    class BeatmapSetInfo {
        [JsonPropertyName("title")] public string Title { get; set; }
        [JsonPropertyName("covers")] public Covers Covers { get; set; }
    }

    class Covers {
        [JsonPropertyName("card@2x")] public string Card { get; set;}
    }

    [Command("test")]
    public async Task TestCommand(CommandContext ctx) {
        if (ctx.Member is not null) {
            return;
        }

        var message = new DiscordEmbedBuilder() {
            Title = "embed",
            Description = "yooo",
            Color = DiscordColor.Rose
        };
        
        await ctx.Channel.SendMessageAsync(message);
    }

    [Command("rs")]
    public async Task RecentScoreCommand(CommandContext ctx) {
        if (ctx.Member is not null && ctx.Member.Username != "ana_rchy") {
            await ctx.Channel.SendMessageAsync("your ass aint ana_rchy :skull:");
            return;
        }

        HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        StringContent postContent = new($"client_id=28077&client_secret={Program.OSU_SECRET}&grant_type=client_credentials&scope=public", Encoding.UTF8, "application/x-www-form-urlencoded");
        
        var postResponse = await httpClient.PostAsync("https://osu.ppy.sh/oauth/token", postContent);
        var token = (await postResponse.Content.ReadAsStringAsync()).Split('"')[9];
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

        var getScoreResponse = await httpClient.GetAsync("https://osu.ppy.sh/api/v2/users/11712494/scores/recent?include_fails=1&mode=osu&limit=1&offset=0");
        var rsJsonString = (await getScoreResponse.Content.ReadAsStringAsync()).Trim("[]".ToCharArray());
        Console.WriteLine(rsJsonString);
        ScoreInfo? scoreInfo = JsonSerializer.Deserialize<ScoreInfo>(rsJsonString);


        var getBeatmapResponse = await httpClient.GetAsync("https://osu.ppy.sh/api/v2/beatmaps/" + scoreInfo.Beatmap.ID);
        var beatmapJsonString = (await getBeatmapResponse.Content.ReadAsStringAsync()).Trim("[]".ToCharArray());
        // Console.WriteLine(beatmapJsonString);
        BeatmapExtendedInfo beatmapExtendedInfo = JsonSerializer.Deserialize<BeatmapExtendedInfo>(beatmapJsonString);



        string modsString = scoreInfo.Mods.Length == 0 ? "" : "+";
        if (modsString == "+") {
            foreach (var i in scoreInfo.Mods) {
                modsString += i;
            }
        }
        scoreInfo.Rank = scoreInfo.Rank == "X" ? "**SS**" : scoreInfo.Rank;
        scoreInfo.Rank = scoreInfo.Rank == "S" ? "**S**" : scoreInfo.Rank;
        scoreInfo.PP = scoreInfo.PP is null ? 0 : scoreInfo.PP;
        var comboString = scoreInfo.Combo == beatmapExtendedInfo.MaxCombo ?
            $"**{scoreInfo.Combo}x/{beatmapExtendedInfo.MaxCombo}x**" :
            $"{scoreInfo.Combo}x/{beatmapExtendedInfo.MaxCombo}x";

        var hitStats = scoreInfo.HitStats;
        var beatmap = scoreInfo.Beatmap;
        var mapCompletion = Math.Round((decimal) (hitStats.Count300 + hitStats.Count100 + hitStats.Count50 + hitStats.CountMiss) / (beatmap.CountCircles + beatmap.CountSliders + beatmap.CountSpinners) * 100, 1);

        DiscordEmbedBuilder embedMessage = new() {
            Title = $"ana_rchy ​ | ​ {scoreInfo.BeatmapSet.Title} [{scoreInfo.Beatmap.Diff}] {modsString} [{scoreInfo.Beatmap.BaseSR}*]",
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() {
                Url = scoreInfo.BeatmapSet.Covers.Card,
                Width = 750,
            },
            Color = DiscordColor.Rose,
            Url = $"https://osu.ppy.sh/b/{scoreInfo.Beatmap.ID}",

            Description = $"\\> {Math.Round(scoreInfo.Accuracy * 100, 2)}% ({scoreInfo.Rank}) ​ | ​ **{Math.Round((float) scoreInfo.PP, 2)}pp** ​ | ​ {mapCompletion}% completion\n" +
            $"\\> {comboString} [{hitStats.Count300}/{hitStats.Count100}/{hitStats.Count50}/{hitStats.CountMiss}] ​ | ​ {scoreInfo.Score}"
        };

        await ctx.Channel.SendMessageAsync(embedMessage);
    }
}