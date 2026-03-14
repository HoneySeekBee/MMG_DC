using StackExchange.Redis;

namespace DCRoomServer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: false);

        // ── Infrastructure ────────────────────────────────────────────────
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]!));

        var app = builder.Build();

        app.UseWebSockets();

        app.MapGet("/", () => "DCRoomServer");

        app.Run();
    }
}
