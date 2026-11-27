using System.Text.Json.Serialization;
using HealthService.API.Middleware;
using HealthService.Application;
using HealthService.Application.Abstractions;
using HealthService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

// ProblemDetails для всіх помилок + централізований обробник винятків
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// кожен шар реєструє свої залежності методом розширення
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// перевірка конфігурації AutoMapper при старті
app.Services.AssertMappingConfiguration();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/ping", () => "HealthService.Version4.0.0");
app.MapHealthChecks("/health");     // health check MongoDB

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
