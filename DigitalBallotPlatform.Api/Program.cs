using DigitalBallotPlatform.Api.Controllers;
using DigitalBallotPlatform.Api.Services;
using DigitalBallotPlatform.DataAccess.Context;
using DigitalBallotPlatform.Domain.Data.Interfaces;
using DigitalBallotPlatform.Domain.Data.Repositories;
using DigitalBallotPlatform.Shared.Logger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using System.Net.WebSockets;
using System.Text;
using ILogger = DigitalBallotPlatform.Shared.Logger.ILogger;

namespace DigitalBallotPlatform.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            //builder.Services.AddAuthentication(IISDefaults.AuthenticationScheme);

            //builder.WebHost.ConfigureKestrel(options =>
            //{
            //    options.ListenLocalhost(5001);
            //    options.ListenLocalhost(7300, listenOpts =>
            //    {
            //        listenOpts.UseHttps();
            //    });
            //});

            // Add services to the container.
            builder.Services.AddSingleton<ILogger, Logger>();
            builder.Services.AddScoped<IBallotCategoryRepo, BallotCategoryRepo>();
            builder.Services.AddScoped<IBallotMaterialRepo, BallotMaterialRepo>();
            builder.Services.AddScoped<IBallotSpecRepo, BallotSpecRepo>();
            builder.Services.AddScoped<IElectionSetupRepo, ElectionSetupRepo>();
            builder.Services.AddScoped<IPartyRepo, PartyRepo>();
            builder.Services.AddScoped<ICompanyRepo, CompanyRepo>();
            builder.Services.AddScoped<ICountyRepo, CountyRepo>();
            builder.Services.AddScoped<IPlatformUserRepo, PlatformUserRepo>();
            builder.Services.AddScoped<IRoleRepo, RoleRepo>();
            builder.Services.AddScoped<IWatermarkColorsRepo, WatermarkColorsRepo>();
            builder.Services.AddScoped<IWatermarkRepo, WatermarkRepo>();
            builder.Services.AddScoped<ITokenService, TokenService>();

            // Add controllers to the container.
            builder.Services.AddScoped<BallotController>();
            builder.Services.AddScoped<ElectionSetupController>();
            builder.Services.AddScoped<StakeholderController>();
            builder.Services.AddScoped<PlatformController>();
            builder.Services.AddScoped<WatermarkController>();
            builder.Services.AddScoped<AuthController>();

            // Add SQL Server connection strings
            var electionDbConnStrSQL = configuration.GetConnectionString("SQLElectionDbConnection");
            // Add PostgreSQL connection strings
            var electionDbConnStrPG = configuration.GetConnectionString("PGElectionDbConnection");

            builder.Services.AddDbContext<ElectionDbContext>(options => 
            options.UseSqlServer(electionDbConnStrSQL)
            .EnableSensitiveDataLogging());

            builder.Services.AddControllers()
                .AddNewtonsoftJson(settings =>
                {
                    settings.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    settings.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                }).AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });
            
            builder.Services.AddScoped<TokenService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    corsBuilder => corsBuilder
                        .WithOrigins("http://localhost:3001") // Express.js frontend server
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            var key = Encoding.ASCII.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.RequireHttpsMetadata = false;
                opt.SaveToken = true;
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"]!,
                    ValidAudience = builder.Configuration["JwtSettings:Audience"]!
                };
            });

            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "JWT Auth API", Version = "v1" });

                var securitySchema = new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                c.AddSecurityDefinition("Bearer", securitySchema);

                var securityRequirement = new OpenApiSecurityRequirement
                {
                    {securitySchema, new[] { "Bearer" } }
                };

                c.AddSecurityRequirement(securityRequirement);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "JWT Auth API v1"));
            }

            app.UseWebSockets();

            app.UseCors("CorsPolicy");

            app.Use(async (context, next) =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await HandleWebSocket(context, webSocket);
                }
                else
                {
                    await next();
                }
            });

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static async Task HandleWebSocket(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                var criteria = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var filteredData = ApplyCriteria(criteria);

                var serverMsg = Encoding.UTF8.GetBytes(filteredData);
                await webSocket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        private static string ApplyCriteria(string criteria)
        {
            return $"Needs to be implemented on how we're going to pass in criteria: {criteria}";
        }
    }
}
