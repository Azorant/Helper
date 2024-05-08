using Helper.Bot.HostedServices;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Helper.Bot;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

try
{
    IConfigurationRoot configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddJsonFile("appsettings.Development.json")
        .Build();
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .CreateLogger();
    var builder = new HostApplicationBuilder();


    builder.Services
        .AddSingleton(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds
        })
        .AddSingleton<DiscordSocketClient>()
        .AddSingleton<InteractionService>()
        .AddHostedService<DiscordClientHost>()
        .AddHostedService<ClientStatus>()
        .AddSerilog()
        .AddDbContext<DatabaseContext>(options =>
        {
            var connectionString = Environment.GetEnvironmentVariable("DATABASE") ?? configuration.GetConnectionString("DATABASE")!;
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        });

    var host = builder.Build();

    using (var scope = host.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        db.Database.Migrate();
    }

    host.Run();
}
catch (HostAbortedException)
{ }
catch (Exception error)
{
    Log.Error(error, "Error in main");
}
finally
{
    Log.CloseAndFlush();
}