using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Helper.Bot;

public class CheckOwnerAttribute : PreconditionAttribute
{
    public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        var client = services.GetService<DiscordSocketClient>();
        if (client == null) return PreconditionResult.FromError("Client null while checking owner");
        var info = await client.GetApplicationInfoAsync();
        if (info.Team == null)
        {
            return info.Owner.Id == context.User.Id ? PreconditionResult.FromSuccess() : PreconditionResult.FromError("Not an owner");
        }
        return info.Team.TeamMembers.Select(m => m.User.Id).Contains(context.User.Id) ? PreconditionResult.FromSuccess() : PreconditionResult.FromError("Not an owner");
    }
}