using Application.Configurations;
using Application.Services.BackgroundServices;
using Application.Services.Implementations;
using Application.Services.Interfaces;
using Domain.Repositories;
using Domain.Services.Implementations;
using Domain.Services.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

namespace Application;

public static class Program
{
    public static async Task Main()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
        Log.Information("StreamingService WebApi is up");
        try
        {
            var builder = WebApplication.CreateBuilder();
            builder.Host.UseSerilog((hbc, conf) =>
            {
                conf.MinimumLevel.Information()
                    .WriteTo.Console()
                    .MinimumLevel.Information();
            });
            var jwtConfig = builder.Configuration
                .GetRequiredSection("JwtConfig")
                .Get<JwtConfig>();
            if (jwtConfig is null)
            {
                throw new InvalidOperationException("JwtConfig is not configured");
            }
            
            builder.Services.AddOptions<JwtConfig>()
                .Bind(builder.Configuration.GetSection("JwtConfig"))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(jwtConfig.SigningKeyBytes),
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        RequireExpirationTime = true,
                        RequireSignedTokens = true,
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        ValidAudiences = new[] { jwtConfig.Audience },
                        ValidIssuer = jwtConfig.Issuer
                    };
                });
            builder.Services.AddAuthorization();
            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. " +
                                  "\r\n\r\n Enter 'Bearer' [space] and then your token in the text input below." +
                                  "\r\n\r\nExample: \"Bearer 1abcabcabc\"",
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            var connectionString = builder
                .Configuration.GetConnectionString("PostgreSQL");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("PostgreSql connection string is not configured");
            builder.Services.AddDbContext<PostgreDbContext>(options =>
                options.UseNpgsql(connectionString));
            
            // builder.Services.AddDbContext<PostgreDbContext>(options =>
            //     options.UseNpgsql(
            //         "Host=localhost;Port=5432;Database=StreamingServiceDb;Username=postgres;Password=lokomotiv960; Include Error Detail=true",
            //         b => b.MigrationsAssembly("Application"))); //для миграций не забыть удалить в конце

            builder.Services.AddScoped(typeof(IRepository<>), typeof(PostgreRepository<>));
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ITrackRepository, TrackRepository>();
            builder.Services.AddScoped<ILastListenedTrackRepository, LastListenedTrackRepository>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ITrackService, TrackService>();
            builder.Services.AddScoped<ILastListenedAlbumRepository, LastListenedAlbumRepository>();
            builder.Services.AddScoped<ILastListenedPlaylistRepository, LastListenedPlaylistRepository>();
            builder.Services.AddScoped<IAlbumRepository, AlbumRepository>();
            builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
            builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
            builder.Services.AddScoped<IPlaylistRepository, PlaylistRepository>();
            builder.Services.AddScoped<IPlaylistTrackRepository, PlaylistTrackRepository>();

            builder.Services.AddSingleton<IPasswordHasherService, PasswordHasherService>();
            builder.Services.AddSingleton<IJwtService, JwtService>();
            builder.Services.AddSingleton<IFileService, FileService>();
            builder.Services.AddHostedService<FreeTracksForUsers>();

            builder.Services.AddCors();
            builder.Services.AddHttpLogging(options =>
            {
                options.LoggingFields = HttpLoggingFields.RequestHeaders
                                        | HttpLoggingFields.ResponseHeaders
                                        | HttpLoggingFields.RequestBody
                                        | HttpLoggingFields.ResponseBody;
            });
            var app = builder.Build();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = string.Empty;
            });
            app.UseCors(policy =>
            {
                policy
                    .AllowCredentials()
                    .AllowAnyMethod()
                    .WithOrigins("http://localhost:5173")
                    .AllowAnyHeader();
            });
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseStaticFiles();
            app.UseHttpLogging();
            app.MapControllers();
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unexpected error");
        }
        finally
        {
            Log.Information("Server shutting down");
            await Log.CloseAndFlushAsync();
        }
    }
}