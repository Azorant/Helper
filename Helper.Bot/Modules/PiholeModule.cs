using Discord;
using Discord.Interactions;
using Serilog;

namespace Helper.Bot.Modules;

[Group("pi-hole", "Commands to manage Pi-hole")]
[CheckOwner]
[CommandContextType(InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel)]
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
public class PiholeModule : InteractionModuleBase<SocketInteractionContext>
{
    private async Task Request(string value)
    {
        var client = new HttpClient();
        using var response =
            await client.GetAsync($"{Environment.GetEnvironmentVariable("PIHOLE_HOST")}/admin/api.php?{value}&auth={Environment.GetEnvironmentVariable("PIHOLE_AUTH")}");
        response.EnsureSuccessStatusCode();
    }

    [SlashCommand("enable", "Enable blocking")]
    public async Task EnableBlocking()
    {
        try
        {
            await Request("enable");
            await RespondAsync(":white_check_mark: Blocking enabled");
        }
        catch (Exception error)
        {
            Log.Error(error, "Error while enabling blocking");
            await RespondAsync($":x: Error occurred while enabling blocking: `{error.Message}`");
        }
    }

    [SlashCommand("disable", "Disable blocking")]
    public async Task DisableBlocking([Summary(description: "How many seconds to disable blocking for"), MinValue(0)] int seconds = 0)
    {
        try
        {
            await Request($"disable{(seconds == 0 ? "" : $"={seconds}")}");
            await RespondAsync($":white_check_mark: Blocking disabled{(seconds == 0 ? "" : $" for {seconds:N0} seconds")}");
        }
        catch (Exception error)
        {
            Log.Error(error, "Error while disabling blocking");
            await RespondAsync($":x: Error occurred while disabling blocking: `{error.Message}`");
        }
    }
}