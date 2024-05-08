using Discord;
using Discord.Interactions;
using Helper.Bot.Entities;
using Microsoft.EntityFrameworkCore;
using TimeSpanParserUtil;

namespace Helper.Bot.Modules;

[Group("reminder", "Reminder yourself something")]
[CommandContextType(InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel)]
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
public class ReminderModule(DatabaseContext db) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("add", "Add a reminder")]
    public async Task AddCommand(string time, [MaxLength(200)] string content)
    {
        var options = new TimeSpanParserOptions
        {
            UncolonedDefault = Units.Seconds,
            FailOnUnitlessNumber = false
        };
        var parsed = TimeSpanParser.Parse(time, options);

        var reminder = new ReminderEntity
        {
            UserId = Context.User.Id,
            Content = content,
            EndsAt = DateTime.UtcNow + parsed
        };
        db.Add(reminder);
        await db.SaveChangesAsync();

        var embed = new EmbedBuilder()
            .WithTitle("Reminder created")
            .WithDescription($"I'll remind you in {TimestampTag.FromDateTime(reminder.EndsAt, TimestampTagStyles.Relative).ToString()} `{Format.Sanitize(content)}`")
            .Build();

        var components = new ComponentBuilder()
            .AddRow(new ActionRowBuilder().WithButton("Cancel reminder", $"reminder:cancel:{reminder.Id.ToString()}", ButtonStyle.Secondary)).Build();
        
        await RespondAsync(embed: embed, components: components);
    }

    [ComponentInteraction("reminder:cancel:*", true)]
    public async Task CancelComponent(string id)
    {
        var reminder = await db.Reminders.FirstOrDefaultAsync(r => r.Id == Guid.Parse(id));
        if (reminder == null || Context.User.Id != reminder.UserId) return;
        await DeferAsync();
        db.Remove(reminder);
        await db.SaveChangesAsync();
        var message = await FollowupAsync("Reminder cancelled");
        await Task.Delay(5000);
        await DeleteOriginalResponseAsync();
        await message.DeleteAsync();
    }
}