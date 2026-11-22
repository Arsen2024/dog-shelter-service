# AdoptionsService — Лаб. 02: тришарова архітектура на ADO.NET та Dapper

Мікросервіс «Заявки на прилаштування» (Проєкт №1): від бази `AdoptionsDb` (Лаб. 01) до REST API.
DAL на ADO.NET і Dapper, бізнес-логіка з транзакціями через Unit of Work, API зі статусами 400/404/409
у форматі ProblemDetails, запуск разом із базою через .NET Aspire.

## 1. Структура рішення

| Шар за методичкою | Проєкт | Призначення |
|---|---|---|
| Api | `AdoptionsService.API` | контролери (без SQL і бізнес-правил), глобальний обробник винятків, Swagger, налаштування хосту |
| Bll | `AdoptionsService.Application` | сервіси, DTO з DataAnnotations, профілі AutoMapper |
| Dal | `AdoptionsService.Infrastructure` | інтерфейси репозиторіїв (Generic Repository), реалізації ADO.NET та Dapper, Unit of Work |
| Domain | `AdoptionsService.Domain` | моделі, read-моделі, статуси, доменні винятки (спільні для DAL і BLL) |
| Aspire | `AdoptionsService.AppHost` | запуск API з базою однією командою |

```
Api -> Bll -> Dal -> Domain   (залежності в один бік; Api бачить лише Bll)
```

## 2. Вимоги для запуску

- .NET 8 SDK (або новіший), Visual Studio 2022 / VS Code
- SQL Server з базою `AdoptionsDb`, розгорнутою скриптами Лаб. 01 (`AdoptionService/database/`: `schema.sql`, `procedures.sql`, `seed.sql`)
- Сертифікат розробки: `dotnet dev-certs https --trust` (потрібен для Aspire Dashboard)

## 3. Запуск

### Через .NET Aspire (рекомендовано — одна команда)

```powershell
dotnet run --project AdoptionService\AdoptionsService.AppHost
```

Відкриється Aspire Dashboard (`https://localhost:17002`). Ресурс `adoptions-api` має статус **Running**;
посилання на його endpoint-и — у Dashboard. Swagger: `<адреса API>/swagger`.

Рядок підключення береться з `ConnectionStrings:AdoptionsDb` у `AdoptionsService.AppHost/appsettings.json`
і передається в API через `WithReference`. Ім'я ресурсу збігається з ім'ям рядка підключення, тому код API не змінюється.

### Без Aspire

```powershell
dotnet run --project AdoptionService\AdoptionsService.API
```

Swagger: `https://localhost:7002/swagger` (або `http://localhost:5002/swagger`).

### Адреси API

Aspire використовує адреси з `launchSettings.json` API: `https://localhost:7002` та `http://localhost:5002`
(у Dashboard вони показані як посилання ресурсу `adoptions-api`). Тому приклади нижче з `http://localhost:5002`
працюють і з Aspire, і без нього. Якщо адреса в Dashboard інша — підставте її.

### Перевизначення рядка підключення змінною оточення

```powershell
$env:ConnectionStrings__AdoptionsDb = "Server=localhost\SQLEXPRESS;Database=AdoptionsDb;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet run --project AdoptionService\AdoptionsService.API
```

## 4. Шар доступу до даних (DAL)

- **`IGenericRepository<T>`**: `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `DeleteAsync` (soft delete, якщо в таблиці є `IsDeleted`).
- **`BaseDapperRepository<T>`**: спільна реалізація CRUD на Dapper. Імена таблиць і стовпців беруться лише з коду, усі **значення** — параметри.
- **Dapper-репозиторії (2):**
  - `AdopterRepository` — CRUD з бази, пошук за email, пагінація, оновлення з оптимістичною конкурентністю (`RowVersion`);
  - `ApplicationRepository` — **multi-mapping** (`GetWithDogsAsync`: заявка + собаки одним запитом), виклики процедур з `CommandType.StoredProcedure` (`usp_CreateApplication`, `usp_AddDogToApplication`, `usp_ChangeApplicationStatus`, `usp_GetApplicationDetails` через `QueryMultipleAsync`), `UPDLOCK`-читання.
  - Також на базі `BaseDapperRepository`: `RequirementRepository`, `DogRepository`.
- **Репозиторій на чистому ADO.NET (1):** `ApplicationStatusHistoryRepository` — `SqlCommand`, `SqlParameter`, `SqlDataReader`; свідомо не успадковує Generic Repository.
- **Unit of Work (`IUnitOfWork`):** реєструється як `Scoped` — одне з'єднання (`IDbSession`) на HTTP-запит. Репозиторії отримують з'єднання та поточну транзакцію з `IDbSession`, а не рядок підключення. `ExecuteInTransactionAsync` = `Begin → action → Commit`, при будь-якому винятку `Rollback`; незавершена транзакція також відкочується при `Dispose` сесії.
- **Параметризація:** жодної конкатенації вхідних даних у SQL.
- **`SqlExceptionTranslator`:** `THROW 50001–50003` → `NotFoundException`; `50004–50010`, `50020`, UNIQUE (`2601`, `2627`), FK/CHECK (`547`) → `BusinessConflictException`. Коди процедур — з Лаб. 01 (вони відрізняються від прикладу методички: у нас `50002` = «заявника не знайдено»).

### Рівень ізоляції транзакцій

За замовчуванням — **`READ COMMITTED`**.

| Рівень | Захищає від | Ціна |
|---|---|---|
| `READ COMMITTED` (обрано) | брудного читання | можливі неповторюване читання й фантоми; мінімум блокувань |
| `REPEATABLE READ` | + неповторюваного читання | утримує shared-блокування до кінця транзакції → більше очікувань |
| `SERIALIZABLE` | + фантомів | діапазонні блокування, найвищий ризик deadlock і найнижча пропускна здатність |

**Компроміс «менше аномалій ↔ більше блокувань».** Чим вищий рівень, тим менше аномалій, але тим більше блокувань і ризик взаємних блокувань. Операції цього сервісу короткі й торкаються окремих заявок, тому критичні місця захищаємо точково: у `SignAgreementAsync` рядок заявки читається з підказкою `UPDLOCK, HOLDLOCK`, і паралельне підписання тієї самої заявки чекає на першу транзакцію. Для заявників застосовано оптимістичну конкурентність (`RowVersion`): конфлікт → `409`.

## 5. Бізнес-логіка (BLL)

- **DTO** для кожного контракту; валідація **на двох рівнях**: DataAnnotations на DTO (відхилення до сервісу, `400`) та правила з даними з БД у сервісі (унікальність email, статус заявки, належність собаки до заявки).
- **AutoMapper 14.0.0** (остання версія без обов'язкового ліцензійного ключа; 15.0+ потребує Community-ключа). Конфігурація перевіряється при **старті сервісу** (`AssertConfigurationIsValid`): з некоректним профілем API не запуститься.
- **Транзакційна операція «підписати договір»** (`POST /api/applications/{id}/agreement`) в одній транзакції: договір (Dapper) → статус `Completed` (Dapper) → запис в історію (ADO.NET). Помилка на будь-якому кроці відкочує все.
- **Знімок даних:** назва собаки при створенні заявки копіюється в `ApplicationDogs.DogName` процедурою `usp_CreateApplication` (політика дублювання з Лаб. 01).

## 6. API і статуси

| Метод і маршрут | Опис | Статус |
|---|---|---|
| `GET /api/adopters`, `GET /api/adopters/{id}` | список (пагінація), деталі | 200 / 404 |
| `POST /api/adopters` | створення | 201 + Location / 400 / 409 |
| `PUT /api/adopters/{id}` | оновлення (потрібен `rowVersion`) | 200 / 404 / 409 |
| `DELETE /api/adopters/{id}` | soft delete | 204 / 404 |
| `GET /api/applications?status=&adopterId=&pageNumber=&pageSize=` | список заявок | 200 / 400 |
| `GET /api/applications/{id}` | заявка з собаками (multi-mapping) | 200 / 404 |
| `GET /api/applications/{id}/details` | деталі (процедура) | 200 / 404 |
| `GET /api/applications/{id}/history` | історія статусів (ADO.NET) | 200 / 404 |
| `POST /api/applications` | створення заявки з першою собакою | 201 + Location / 400 / 404 / 409 |
| `POST /api/applications/{id}/dogs` | додати собаку | 204 / 404 / 409 |
| `POST /api/applications/{id}/status` | зміна статусу (процедура з перевіркою переходу) | 204 / 404 / 409 |
| `POST /api/applications/{id}/requirements/{requirementId}/fulfill` | позначити вимогу виконаною | 204 / 404 |
| `POST /api/applications/{id}/agreement` | підписання договору (транзакція) | 204 / 404 / 409 |
| `GET /api/requirements`, `GET /api/dogs`, `PUT /api/internal/dogs/{id}` | довідники; імітація події `DogUpdated` | 200 / 204 / 400 |

Усі помилки — `application/problem+json` з полями `status`, `title`, `detail`, `traceId`.
Для аудиту використовується заголовок `X-User-Name` (за замовчуванням `system`).

## 7. Демонстрація

Нижче `{id}` — `id` схваленої заявки з `GET /api/applications?status=Approved`.

> Приклади `curl` призначені для **bash** (Git Bash, WSL, Linux/macOS). У Windows PowerShell `curl` — це псевдонім
> `Invoke-WebRequest`, тому там використовуйте Swagger або `Invoke-RestMethod` / `curl.exe` (приклад — одразу нижче).

```powershell
# PowerShell: створення заявника зі спецсимволом в імені (апостроф у рядку подвоюється)
Invoke-RestMethod -Method Post -Uri http://localhost:5002/api/adopters -ContentType "application/json" `
  -Body '{"fullName":"Patrick O''Brien","email":"obrien@example.com"}'
```

```bash
# 400 — помилка валідації DataAnnotations
curl -i -X POST http://localhost:5002/api/adopters -H "Content-Type: application/json" -d '{"fullName":"","email":"not-an-email"}'

# значення зі спецсимволом не ламає SQL (параметризація)
curl -i -X POST http://localhost:5002/api/adopters -H "Content-Type: application/json" -d '{"fullName":"Patrick O'"'"'Brien","email":"obrien@example.com"}'

# 409 — той самий email удруге
curl -i -X POST http://localhost:5002/api/adopters -H "Content-Type: application/json" -d '{"fullName":"Another","email":"obrien@example.com"}'

# 404 — неіснуючий Id
curl -i http://localhost:5002/api/applications/00000000-0000-0000-0000-000000000001

# ВІДКАТ ТРАНЗАКЦІЇ: усі записи виконано, потім штучна помилка -> 409, у БД нічого не змінилося
curl -i -X POST "http://localhost:5002/api/applications/{id}/agreement?simulateFailure=true" \
  -H "Content-Type: application/json" \
  -d '{"dogId":"d0000000-0000-0000-0000-000000000002","agreementNumber":"AGR-ROLLBACK","signedBy":"Андрій Мельник"}'
```

Перевірка відкату в SQL: заявка лишається `Approved`, договору й нового запису історії немає:

```sql
SELECT Status FROM dbo.AdoptionApplications WHERE Id = '<id>';
SELECT * FROM dbo.AdoptionAgreements WHERE ApplicationId = '<id>';              -- 0 рядків
SELECT * FROM dbo.ApplicationStatusHistory WHERE ApplicationId = '<id>';         -- без запису "Agreement AGR-ROLLBACK signed"
```

```bash
# УСПІШНА транзакція: 204, статус стає Completed, з'являються договір і запис історії
curl -i -X POST "http://localhost:5002/api/applications/{id}/agreement" \
  -H "Content-Type: application/json" \
  -d '{"dogId":"d0000000-0000-0000-0000-000000000002","agreementNumber":"AGR-2026-0001","signedBy":"Андрій Мельник"}'

# 409 — повторне підписання тієї самої заявки (вже Completed)
curl -i -X POST "http://localhost:5002/api/applications/{id}/agreement" \
  -H "Content-Type: application/json" \
  -d '{"dogId":"d0000000-0000-0000-0000-000000000002","agreementNumber":"AGR-2026-0002","signedBy":"Андрій Мельник"}'
```

Прапорець `simulateFailure` працює лише в середовищі Development.