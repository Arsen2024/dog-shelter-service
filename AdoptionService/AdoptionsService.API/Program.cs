using System.Threading.RateLimiting;
using AdoptionsService.API.Middleware;
using AdoptionsService.Application;
using AdoptionsService.Domain.Interfaces;
using AdoptionsService.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("fixed", o =>
    {
        o.Window = TimeSpan.FromSeconds(5);
        o.PermitLimit = 10;
        o.QueueLimit = 0;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

var connectionString = builder.Configuration.GetConnectionString("AdoptionsDatabase")
    ?? throw new InvalidOperationException("Connection string 'AdoptionsDatabase' is missing.");

builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/ping", () => "AdoptionsService.Version1.0.0").RequireRateLimiting("fixed");

app.UseHttpsRedirection();
app.UseRateLimiter();
app.MapControllers();

app.Run();
