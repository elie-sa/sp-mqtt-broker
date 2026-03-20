using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MQTTnet;
using MQTTnet.Client;
using SPBackend.Data;
using SPBackend.Middleware.Exceptions;
using SPBackend.Services.CurrentUser;
using SPBackend.Services.Mains;
using SPBackend.Services.Commands;
using SPBackend.Services.Mqtt;
using SPBackend.Services.Outbox;
using SPBackend.Services.Plugs;
using SPBackend.Services.Rooms;
using SPBackend.Services.LLM;
using SPBackend.Services.Notifications;
using System.Net;
using System.Net.Sockets;

static string GetLocalIpv4()
{
    return Dns.GetHostEntry(Dns.GetHostName())
        .AddressList
        .First(ip =>
            ip.AddressFamily == AddressFamily.InterNetwork &&
            !IPAddress.IsLoopback(ip))
        .ToString();
}

var builder = WebApplication.CreateBuilder(args);

var localIp = GetLocalIpv4();

var keycloakIssuer = $"http://{localIp}:8080/realms/local";
var keycloakMetadataUrl = $"{keycloakIssuer}/.well-known/openid-configuration";
var keycloakAuthorizationUrl = $"{keycloakIssuer}/protocol/openid-connect/auth";

builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = keycloakIssuer;
        options.MetadataAddress = keycloakMetadataUrl;
        options.Audience = "account";
        options.RequireHttpsMetadata = false;
    });

Console.WriteLine($"[DEV] Using Keycloak issuer: {keycloakIssuer}");

builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });

    c.AddSecurityDefinition("Keycloak", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Implicit = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri(keycloakAuthorizationUrl),
                Scopes = new Dictionary<string, string>
                {
                    ["openid"] = "OpenID",
                    ["profile"] = "Profile"
                }
            }
        }
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Keycloak" }
            },
            new List<string>()
        }
    });
});

builder.Services.AddSingleton<IMqttService, MqttService>();
builder.Services.AddHostedService<MqttHostedService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.Configure<CommandInboxOptions>(builder.Configuration.GetSection("CommandInbox"));
builder.Services.AddHostedService<CommandsHubClient>();

// Gemini Integration
builder.Services.AddSingleton<GeminiService>();
builder.Services.AddScoped<LlmFunctionRouter>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");
var remoteConnectionString = builder.Configuration.GetConnectionString("RemotePostgresConnection");
var outboxEnabled = builder.Configuration.GetValue("Outbox:Enabled", true);

builder.Services.AddDbContext<IAppDbContext, AppDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.Configure<OutboxOptions>(builder.Configuration.GetSection("Outbox"));

if (outboxEnabled && !string.IsNullOrWhiteSpace(remoteConnectionString))
{
    builder.Services.AddDbContext<RemoteDbContext>(options =>
        options.UseNpgsql(remoteConnectionString));
    builder.Services.AddHostedService<OutboxProcessor>();
}

// Global Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Controller Services
builder.Services.AddScoped<PowerSourceService>();
builder.Services.AddScoped<RoomsService>();
builder.Services.AddScoped<PlugsService>();
builder.Services.AddScoped<NotificationService>(); // TODO: Double check if scoped or singleton

// Adding Hangfire
builder.Services.AddHangfire(config =>
    config.UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseMemoryStorage());
builder.Services.AddHangfireServer();

// Allow all frontend endpoints
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.WebHost.UseUrls("http://0.0.0.0:5000");

var app = builder.Build();

app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler(options => {});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization(); 

app.UseHangfireDashboard("/hangfire");

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();