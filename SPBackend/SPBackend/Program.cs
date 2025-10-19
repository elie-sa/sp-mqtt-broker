using System.Text;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using SPBackend.Data;
using SPBackend.Middleware.Exceptions;
using SPBackend.Services.Mains;
using SPBackend.Services.MQTTService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<MqttService>();

var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");

builder.Services.AddDbContext<IAppDbContext, AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Global Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Controller Services
builder.Services.AddScoped<PowerSourceService>();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler(options => {});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();