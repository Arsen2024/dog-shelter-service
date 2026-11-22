var builder = DistributedApplication.CreateBuilder(args);

// Наявна база AdoptionsDb (розгорнута скриптами з Лаб. 01 на локальному SQL Server).
// Значення береться з ConnectionStrings:AdoptionsDb в appsettings.json цього проєкту.
var adoptionsDb = builder.AddConnectionString("AdoptionsDb");

// Ім'я ресурсу = ім'я рядка підключення, тому код API не змінюється.
builder.AddProject<Projects.AdoptionsService_API>("adoptions-api")
       .WithReference(adoptionsDb);

builder.Build().Run();
