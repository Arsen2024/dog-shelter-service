# DogsService — Лаб. 03: тришарова архітектура на Entity Framework Core

Мікросервіс «Каталог собак» (Проєкт №2 з Лаб. 01): Code-First і Fluent API, Generic Repository + три
специфічні репозиторії (Eager / Explicit / LINQ через M:N), Unit of Work, Specification pattern (Ardalis),
BLL з AutoMapper, валідація (DataAnnotations + FluentValidation), глобальна обробка помилок (ProblemDetails),
пагінація/фільтрація/сортування на рівні БД, Serilog.

## 1\. Структура рішення

|Шар за методичкою|Проєкт|Призначення|
|-|-|-|
|Api|`DogsService.API`|контролери, глобальний обробник винятків, Serilog, Swagger, сідінг при старті|
|Bll|`DogsService.Application`|сервіси бізнес-операцій, DTO, профілі AutoMapper, валідатори FluentValidation|
|Dal|`DogsService.Infrastructure`|EF Core (DbContext, Fluent API, міграції), Generic Repository, репозиторії, специфікації, Unit of Work|
|Domain|`DogsService.Domain`|сутності з бізнес-правилами, винятки, read-моделі|

```
Api -> Bll -> Dal -> Domain      (залежності в один бік; Api бачить лише Bll)
```

## 2\. Вимоги

.NET 8 SDK (або новіший), SQL Server (`localhost\\SQLEXPRESS`), Visual Studio 2022 / VS Code,
`dotnet-ef` (для CLI): `dotnet tool install --global dotnet-ef --version 9.0.1`.
Рядок підключення — `DogsService.API/appsettings.json` → `ConnectionStrings:DogsDatabase`
(перевизначається змінною `ConnectionStrings\_\_DogsDatabase`).

## 3\. Міграції і база даних

Package Manager Console (Default project: `DogsService.Infrastructure`, Startup project: `DogsService.API`):

```powershell
Add-Migration InitialCreate -Project DogsService.Infrastructure -StartupProject DogsService.API
Update-Database -Project DogsService.Infrastructure -StartupProject DogsService.API
Script-Migration -Idempotent -Output database\\catalog\_schema.sql -Project DogsService.Infrastructure -StartupProject DogsService.API
```

Те саме через CLI з кореня репозиторію:

```powershell
dotnet ef migrations add InitialCreate --project DogsService.Infrastructure --startup-project DogsService.API
dotnet ef database update --project DogsService.Infrastructure --startup-project DogsService.API
dotnet ef migrations script --idempotent -o database/catalog\_schema.sql --project DogsService.Infrastructure --startup-project DogsService.API
```

Папка `DogsService.Infrastructure/Migrations` і `database/catalog\_schema.sql` входять до репозиторію.
У створеній базі: складений ключ `DogFeatures (DogId, FeatureId)`, `CHECK` на вагу/довжину хвоста/статус/стать,
унікальний фільтрований індекс `IX\_Dogs\_Name\_Active` (аналог унікального `Sku`).

Чисте перерозгортання: `Drop-Database` (або `dotnet ef database drop -f ...`) → `Update-Database`.

## 4\. Запуск і сідінг

```powershell
dotnet run --project DogsService.API
```

Swagger: `https://localhost:7293/swagger` (профіль `https` у `launchSettings.json`).

* **Довідники** (породи, вольєри, характеристики) — статично через `HasData` з фіксованими Id (у міграції).
* **Собаки, профілі, зв'язки собака-характеристика** — програмно (`DbSeeder`) у `Program.cs` після `MigrateAsync()`.
Сідінг ідемпотентний: кожна собака додається лише якщо її Id ще немає, тож повторний запуск не дублює дані.
* `Seeding:Enabled` і `Database:MigrateOnStartup` у `appsettings.Production.json` вимкнені.
* Ті самі Id собак (`d0000000-0000-0000-0000-00000000000X`) використовують AdoptionsService і HealthService.

## 5\. Модель (Part 1)

* Сутності з **приватними setter-ами**; інваріанти перевіряють конструктор і методи (`DomainException`):
вага > 0 і ≤ 2 знаки після коми, `ChangeWeight`, `ChangeStatus` (дозволені переходи `Available→Reserved/UnderTreatment`,
`Reserved→Available/Adopted`, `UnderTreatment→Available`, `Adopted` — кінцевий), `AssignToKennel`, `SetProfile`, `AddFeature`.
* Конфігурація кожної сутності — окремий `IEntityTypeConfiguration<T>` у `Infrastructure/Configurations`;
підключаються одним викликом `ApplyConfigurationsFromAssembly`.
* **1:1** `Dog ↔ DogProfile` (FK = PK), **1:N** `Breed → Dogs`, `Kennel → Dogs`,
**M:N** `Dog ↔ Feature` через явну сутність `DogFeature` з власними полями `AssignedAt`, `Notes`.
* Аудит, soft delete (`IsDeleted` + глобальний query filter), оптимістична конкурентність (`RowVersion`).

## 6\. DAL (Part 2)

* **`EfRepository<T>`** — Async Generic Repository: `AddAsync`, `GetByIdAsync`, `GetAllAsync`, `Update`, `Delete`, `SaveChangesAsync`.
`Update`/`Delete` лише змінюють стан у контексті.
* **Eager Loading** — `DogRepository.GetDetailsAsync`: `Include`/`ThenInclude` у специфікації `DogWithDetailsSpec` → **один** SQL-запит.
* **Explicit Loading** — `BreedRepository.GetWithDogsAsync`: спершу порода, потім `Entry(breed).Collection(b => b.Dogs).LoadAsync()` → **два** запити.
* **LINQ to Entities через M:N** — `DogFeatureRepository.GetAssignmentsAddedAfterAsync`: вибірка за проміжною сутністю та її полем `AssignedAt`.
* **Unit of Work** — об'єднує репозиторії; `SaveChangesAsync()` викликається один раз наприкінці бізнес-операції.
* **Specification pattern (Ardalis.Specification)** — `DogsFilterSpec`, `DogWithDetailsSpec`, `DogByNameSpec`, `DogsByKennelSpec`.
Репозиторій застосовує будь-яку специфікацію в одному місці (`WithSpecification`), нові специфікації не потребують змін у репозиторії
(`DogByNameSpec` і `DogsByKennelSpec` працюють через ті самі `FirstOrDefaultAsync/CountAsync/AnyAsync`).

## 7\. BLL (Part 3)

* Сервіси (`DogService`, `BreedService`, `KennelService`, `FeatureService`) працюють лише через Unit of Work.
Правила сутності перевіряє сутність; сервіс — правила з даними з БД: унікальність імені → `ConflictException`,
наявність породи/вольєра → `EntityNotFoundException`, місткість вольєра → `ConflictException`.
* Бізнес-операції: створення собаки, зміна ваги (аналог «зміни ціни»), зміна статусу, розміщення у вольєрі
(аналог «списання залишку»), додавання характеристики, видалення (soft delete).
* DTO і профіль **AutoMapper 14.0.0** (остання версія без ліцензійного ключа); конфігурація перевіряється при старті
(`AssertConfigurationIsValid`).

## 8\. API

|Метод і маршрут|Опис|Статуси|
|-|-|-|
|`GET /api/dogs?page=\&pageSize=\&breedId=\&kennelId=\&status=\&search=\&sortBy=\&sortDir=`|сторінка собак|200 / 400|
|`GET /api/dogs/{id}`|картка собаки (Eager, 1 запит)|200 / 404|
|`POST /api/dogs`|створення|201 + Location / 400 / 404 / 409|
|`PUT /api/dogs/{id}/weight`|зміна ваги|204 / 400 / 404|
|`PUT /api/dogs/{id}/status`|зміна статусу|204 / 400 / 404 / 409|
|`PUT /api/dogs/{id}/kennel`, `DELETE /api/dogs/{id}/kennel`|вольєр|204 / 404 / 409|
|`POST /api/dogs/{id}/features`|додати характеристику|204 / 404 / 409|
|`DELETE /api/dogs/{id}`|soft delete|204 / 404|
|`GET /api/breeds`, `GET /api/breeds/{id}?includeDogs=`|породи (Explicit Loading при `includeDogs=true`)|200 / 404|
|`GET /api/kennels`|вольєри з заповненістю|200|
|`GET /api/features`, `GET /api/features/{id}/dogs?addedAfter=`|характеристики; собаки, додані після дати (M:N)|200 / 404|

### Пагінація, фільтрація, сортування

* `page` (з 1), `pageSize` (за замовчуванням 10, максимум 100); фільтри `breedId`, `kennelId`, `status`, `search`;
`sortBy` ∈ `name | color | tailLength | weight | createdAt`, `sortDir` ∈ `asc | desc` (інакше 400).
* Відповідь: `items`, `totalCount`, `page`, `pageSize`, `totalPages`, `hasNext`, `hasPrevious`.
* `Where`, `OrderBy`, `Skip/Take` будуються до `ToListAsync()` — база повертає лише сторінку;
`totalCount` — окремий `COUNT` за тими самими критеріями (`page=2\&pageSize=2` → `totalCount = 5`, а не 2).

### Валідація і помилки

* Прості правила — **DataAnnotations** на DTO (обов'язковість, довжини, діапазони, регулярні вирази).
Складні — **FluentValidation** (кількість і довжина кольорів, точність ваги, дата народження), без дублювання.
Валідатори викликаються явно (`IValidator<T>.ValidateAndThrowAsync`); застарілий `FluentValidation.AspNetCore` не використовується.
* Глобальний `IExceptionHandler` → `ProblemDetails` з `traceId` і `correlationId`:
`EntityNotFoundException` → 404, `ValidationException` → 400 (`ValidationProblemDetails` з помилками по полях),
`ConflictException`, `DomainException`, `DbUpdateConcurrencyException`, порушення UNIQUE → 409, решта → 500
(деталі винятку — лише в Development). Кожен виняток логується один раз.

## 9\. Логування (Serilog)

Sink-и: Console і File (`logs/dogs-.log`, щоденна ротація). До кожного запису в межах запиту додаються властивості
`CorrelationId` (заголовок `X-Correlation-Id` або згенерований), `UserId` (заголовок `X-User-Name`, за замовчуванням `system`),
`RequestPath`. Рівень `Information` для `Microsoft.EntityFrameworkCore.Database.Command` показує SQL-запити.
Папку `logs/` додайте в `.gitignore`.

## 10\. Демонстрація

|Що перевірити|Запит|Очікування|
|-|-|-|
|201 + Location|`POST /api/dogs`|`201`, заголовок `Location: .../api/dogs/{id}`|
|409 — те саме ім'я|повтор `POST /api/dogs`|`409`|
|404|`GET /api/dogs/00000000-0000-0000-0000-000000000001`|`404`|
|400 з помилками по полях|`POST /api/dogs` з `"name": ""`|`400`, `errors: { Name: \[...] }`|
|Пагінація|`GET /api/dogs?page=2\&pageSize=2\&sortBy=weight\&sortDir=desc`|2 записи, `totalCount: 5`, `totalPages: 3`|
|Eager → 1 SQL|`GET /api/dogs/{id}`|у лозі **один** `Executed DbCommand`|
|Explicit → 2 SQL|`GET /api/breeds/{id}?includeDogs=true`|у лозі **два** `Executed DbCommand` (порода, потім собаки)|
|M:N через AssignedAt|`GET /api/features/dddddddd-dddd-dddd-dddd-dddddddddddd/dogs?addedAfter=2026-01-15`|Max і Luna|
|Недопустимий перехід|`PUT /api/dogs/d0000000-0000-0000-0000-000000000005/status` `{"status":"Available"}`|`409` (Adopted — кінцевий статус)|
|Ідемпотентний сідінг|перезапустити застосунок|у лозі `Seeding skipped`, дані не дублюються|

Приклад тіла `POST /api/dogs`:

```json
{
  "name": "Buddy",
  "colors": \["Golden", "White"],
  "tailLength": 30,
  "weight": 25.5,
  "breedId": "11111111-1111-1111-1111-111111111111",
  "kennelId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "profile": { "birthDate": "2023-05-01", "gender": "Male", "description": "Friendly", "temperament": "Calm" }
}
```

## 11\. Git

Гілки `main` / `develop` / `feature/lab-03`; коміти за кроками (domain → dal → bll → api → документація);
злиття `feature/lab-03 → develop` через Pull Request з описом.

