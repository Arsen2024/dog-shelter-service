using DogsService.API.Middleware;
using DogsService.Application;
using DogsService.Domain.Interfaces;
using DogsService.Infrastructure.DbContext;
using DogsService.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog: структуровані логи (Console + File), властивості CorrelationId / UserId / RequestPath через LogContext.
// Рівень Information для Microsoft.EntityFrameworkCore.Database.Command показує SQL (див. appsettings.json).
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

// ProblemDetails для всіх помилок + централізований обробник винятків
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var connectionString = builder.Configuration.GetConnectionString("DogsDatabase")
    ?? throw new InvalidOperationException("Connection string 'DogsDatabase' is not configured.");

builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddApplication(connectionString);   // BLL + DAL (DbContext, репозиторії, Unit of Work)

var app = builder.Build();

// перевірка конфігурації AutoMapper при старті
app.Services.AssertMappingConfiguration();

// Міграції + програмний сідінг (довідники — через HasData у міграції).
// Сідінг ідемпотентний і вимикається налаштуванням Seeding:Enabled (в Production — вимкнено).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (app.Configuration.GetValue("Database:MigrateOnStartup", true))
        await db.Database.MigrateAsync();

    if (app.Configuration.GetValue("Seeding:Enabled", false))
        await DbSeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>());
}

app.UseMiddleware<RequestContextMiddleware>();
app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/ping", () => "DogsService.Version3.0.0");

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
