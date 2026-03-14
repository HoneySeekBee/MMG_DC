using System.Text;
using DCData;
using DCData.Auth;
using DCData.Connections;
using DCData.Querying;
using DCData.Repositories.Auth;
using DCData.Security;
using DCServerCore.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
            builder.Services.AddSingleton<IRefreshTokenStore, RefreshTokenStore>();
            builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            // ── JWT Authentication ───────────────────────────────────────────
            var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is required");
            var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MMG_DC";
            var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MMG_DC";

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                        ClockSkew = TimeSpan.Zero
                    };
                });

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

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
