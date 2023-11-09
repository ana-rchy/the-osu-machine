using DSharpPlus;
using DSharpPlus.CommandsNext;

public partial class Program {
    const string TOKEN = "";
    const string PREFIX = ">>";
    public const strng OSU_SECRET = "";
    public const long ANA_OSU_ID = 11712494;

    static DiscordClient Client;
    static CommandsNextExtension Commands;

    static async Task Main(string[] args) {
        var discordConfig = new DiscordConfiguration() {
            Intents = DiscordIntents.All,
            Token = TOKEN,
            TokenType = DSharpPlus.TokenType.Bot,
            AutoReconnect = true  
        };

        Client = new DiscordClient(discordConfig);
        Client.Ready += ClientReady;

        var commandsConfig = new CommandsNextConfiguration() {
            StringPrefixes = new string[] { PREFIX },
            EnableMentionPrefix = true,
            EnableDms = true,
            EnableDefaultHelp = false
        };
        Commands = Client.UseCommandsNext(commandsConfig);
        Commands.RegisterCommands<Commands>();

        await Client.ConnectAsync();
        await Task.Delay(-1);
    }


    static Task ClientReady(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs args) {
        return Task.CompletedTask;
    }
}