using DogsService.Application.Mappings;
using DogsService.Domain.Interfaces;
using DogsService.Infrastructure.DbContext;
using DogsService.Infrastructure.Repositories;
using DogsService.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using FluentValidation.AspNetCore;
using DogsService.Application.Validators;
using FluentValidation;
using DogsService.API.Middleware;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using DogsService.Application.Commands.Dog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("fixed", options =>
    {
        options.Window = TimeSpan.FromSeconds(5);
        options.PermitLimit = 10;
        options.QueueLimit = 0;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DogsDatabase")));

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateDogCommandValidator>();

builder.Services.AddScoped<IDogRepository, DogRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddAutoMapper(typeof(DogMappingProfile).Assembly);
builder.Services.AddMediatR(options =>
    options.RegisterServicesFromAssembly(typeof(CreateDogCommandHandler).Assembly));

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/ping", () => "Dogshouseservice.Version1.0.1").RequireRateLimiting("fixed");

app.UseHttpsRedirection();

app.UseRateLimiter();

app.MapControllers();

app.Run();
