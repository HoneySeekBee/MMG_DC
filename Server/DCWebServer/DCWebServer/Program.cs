using DCData;
using DCData.Connections;
using DCData.Querying;
using DCData.Repositories.Auth;
using DCData.Security;
using DCData.Session;
using DCServerCore.Auth;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using StackExchange.Redis;

namespace DCWebServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // secrets.json(비밀번호 등) — .gitignore 대상, optional
            builder.Configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: false);

            // ── Infrastructure ────────────────────────────────────────────────
            builder.Services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();
            builder.Services.AddScoped<IQueryFactoryProvider, QueryFactoryProvider>();
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]!));

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(
                    builder.Configuration.GetConnectionString("MainDatabase"),
                    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MainDatabase"))
                ));

            // ── Repositories & Services ──────────────────────────────────────
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddSingleton<ISessionStore, SessionStore>();
            builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            // ── API ──────────────────────────────────────────────────────────
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
