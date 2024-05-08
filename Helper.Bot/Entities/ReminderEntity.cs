using System.ComponentModel.DataAnnotations;

namespace Helper.Bot.Entities;

public class ReminderEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public required ulong UserId { get; set; }
    [MaxLength(256)]
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public required DateTime EndsAt { get; set; }
}