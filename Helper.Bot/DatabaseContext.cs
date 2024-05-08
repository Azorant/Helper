using Helper.Bot.Entities;
using Microsoft.EntityFrameworkCore;

namespace Helper.Bot;

public class DatabaseContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<ReminderEntity> Reminders { get; set; }
}