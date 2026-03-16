using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SPBackend.Data;
using SPBackend.Middleware.Exceptions;
using SPBackend.Services.CurrentUser;
using SPBackend.Services.Mains;
using SPBackend.Services.Commands;
using SPBackend.Services.Mqtt;
using SPBackend.Services.Outbox;
using SPBackend.Services.Plugs;
using SPBackend.Services.Rooms;

var builder = WebApplication.CreateBuilder(args);

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
                AuthorizationUrl = new Uri(builder.Configuration["Keycloak:AuthorizationUrl"]),
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
builder.Services.AddScoped<ScheduleJobService>();
builder.Services.AddHostedService<ScheduleHangfireBootstrapper>();

//Adding Hangfire
builder.Services.AddHangfire(config =>
    config.UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseMemoryStorage());
builder.Services.AddHangfireServer();

// Adding authentication
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.Audience = builder.Configuration["Authentication:Audience"];
        options.MetadataAddress = builder.Configuration["Authentication:MetadataAddress"];
         

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Authentication:ValidIssuer"]
        };
    });

var app = builder.Build();
app.UseRouting();
app.UseSwagger();
app.UseSwaggerUI();

app.UseExceptionHandler(_ => { });

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire");

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = (AppDbContext)scope.ServiceProvider.GetRequiredService<IAppDbContext>();
    db.Database.Migrate();
}

app.Run();
