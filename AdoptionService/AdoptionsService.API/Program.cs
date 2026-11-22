using AdoptionsService.API.Middleware;
using AdoptionsService.Application;
using AdoptionsService.Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

// ProblemDetails для всіх помилок (400 від DataAnnotations, 404 маршрутів, виняткові ситуації) + traceId
builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = ctx =>
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Ім'я рядка підключення збігається з ім'ям ресурсу в Aspire AppHost (AddConnectionString("AdoptionsDb")).
// Перевизначається змінною оточення ConnectionStrings__AdoptionsDb.
var connectionString = builder.Configuration.GetConnectionString("AdoptionsDb")
    ?? throw new InvalidOperationException("Connection string 'AdoptionsDb' is not configured.");

builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddApplication(connectionString);   // BLL + DAL (Unit of Work — Scoped)

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

app.MapGet("/ping", () => "AdoptionsService.Version2.0.0");

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
