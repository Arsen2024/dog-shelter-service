# Dog Shelter Management System — три бази даних для мікросервісів

Практичне заняття №01: проєктування баз даних для мікросервісної архітектури.
Предметна область — **притулок для собак**.

## 1. Предметна область

Система керування притулком. Співробітники ведуть облік собак, порід і вольєрів; люди подають заявки на прилаштування, які проходять перевірку й завершуються договором; ветеринари ведуть медичні картки, вакцинації та візити.

## 2. Структура репозиторію

```
DogsService.sln                      Каталог собак (Проєкт №2, EF Core)
DogsService.API / .Application / .Domain / .Infrastructure
database/catalog_schema.sql          SQL-схема Каталогу (генерується з міграцій, див. п. 7.1)
AdoptionService/                     Заявки на прилаштування (Проєкт №1, ADO.NET + Dapper)
  AdoptionsService.sln
  AdoptionsService.API / .Application / .Domain / .Infrastructure / .Tests
  database/                          schema.sql, procedures.sql, seed.sql, queries.sql, security.sql
HealthService/                       Медичні картки (Проєкт №3, MongoDB)
  HealthService.sln
  HealthService.API / .Application / .Domain / .Infrastructure / .Tests
  database/                          init.js, seed.js, queries.js, samples.json
docs/erd/                            ERD: catalog.dbml, adoptions.dbml (+ зображення)
docker-compose.yml                   опційні SQL Server і MongoDB у Docker
```

## 3. Bounded Contexts

| # | Сервіс | Проєкт | Технологія | Сутності / колекції (кожна належить рівно одному контексту) |
|---|---|---|---|---|
| 1 | **DogsService** (Каталог) | №2 | SQL Server + EF Core Code First | Dog, Breed, Kennel, DogProfile, Feature, DogFeature |
| 2 | **AdoptionsService** | №1 | SQL Server + ADO.NET/Dapper + збережувані процедури | Adopter, AdoptionApplication, ApplicationDog, ApplicationStatusHistory, AdoptionAgreement, Requirement, ApplicationRequirement (+ локальна копія Dogs) |
| 3 | **HealthService** | №3 | MongoDB | medicalRecords, vaccinations, veterinaryVisits |

Кожен сервіс має власну БД і автономну модель. Між базами **немає жодного FOREIGN KEY**: сервіси знають про собаку лише її ідентифікатор (`dogId`).

### Агрегати
- **Каталог:** Dog = Dog + DogProfile (1:1) + DogFeature[] (M:N). Breed, Kennel, Feature — довідники.
- **Adoptions:** AdoptionApplication = Application + ApplicationDog[] + StatusHistory[] + ApplicationRequirement[] + Agreement (1:1). Adopter — окремий агрегат.
- **Health:** MedicalRecord (вбудовані allergies / chronicConditions / notes), Vaccination (вбудований vaccines[]), VeterinaryVisit (reference на картку).

## 4. Політика дублювання даних і узгодженості

| Поле | Де зберігається | Джерело (власник) | Політика узгодженості |
|---|---|---|---|
| `ApplicationDogs.DogName` | AdoptionsDb | `DogsDb.Dogs.Name` | **знімок** на момент подання заявки, не оновлюється |
| `Dogs.Name`, `Dogs.Status` (локальна копія) | AdoptionsDb | `DogsDb.Dogs` | **копія для читання**, оновлюється подіями `DogUpdated` від DogsService (eventual consistency, Лаб. 08) |
| `Dogs.Id` (локальна копія) | AdoptionsDb | `DogsDb.Dogs.Id` | той самий Id, без FK на DogsDb |
| `medicalRecords.dogName`, `vaccinations.dogName`, `veterinaryVisits.dogName` | HealthDb | `DogsDb.Dogs.Name` | **копія для читання**, оновлюється подіями `DogUpdated` |
| `veterinaryVisits.dogSnapshot.{name, breed, weightKg}` | HealthDb | `DogsDb.Dogs` / `Breeds` | **знімок** на момент візиту, не оновлюється |
| `medicalRecordId` у vaccinations / visits | HealthDb | `HealthDb.medicalRecords` | внутрішнє reference в межах БД, цілісність перевіряє застосунок |
| `DogsDb.Dogs.Status` (`Reserved`, `Adopted`) | DogsDb | **DogsDb** (власник) | змінюється DogsService за подіями `ApplicationApproved` / `AgreementSigned` від Adoptions; Adoptions у цю БД не пише (немає dual write) |
| `DogsDb.Dogs.Status` (`UnderTreatment`) | DogsDb | **DogsDb** (власник) | змінюється DogsService за подією від HealthService |

Поки брокера повідомлень немає (Лаб. 08), події імітують внутрішні endpoint-и:
`PUT /api/internal/dogs/{id}` (Adoptions) і `PUT /api/internal/dogs/{dogId}/name` (Health).

## 5. Вибір ключів

- **DogsDb, AdoptionsDb — `UNIQUEIDENTIFIER`.** Id собаки створюється в одному сервісі, а використовується в інших; GUID не потребує центральної послідовності. В Adoptions за замовчуванням `NEWSEQUENTIALID()` (менша фрагментація індексу).
- `ApplicationStatusHistory.Id` — `BIGINT IDENTITY`: один писач, послідовна вставка, це лише журнал.
- **HealthDb — `ObjectId`**, а `dogId` — рядок-UUID із Каталогу.

## 6. Зв'язки

### DogsDb
| Зв'язок | Тип | Де FK | При видаленні |
|---|---|---|---|
| Breeds → Dogs | 1:N | `Dogs.BreedId` | `Restrict` |
| Kennels → Dogs | 1:N | `Dogs.KennelId` (nullable) | `SetNull` |
| Dogs ↔ DogProfiles | 1:1 | `DogProfiles.DogId` (= PK) | `Cascade` |
| Dogs ↔ Features | M:N | `DogFeatures (DogId, FeatureId)`, поля `AssignedAt`, `Notes` | `Cascade` / `Restrict` |

### AdoptionsDb
| Зв'язок | Тип | Де FK | При видаленні |
|---|---|---|---|
| Adopters → AdoptionApplications | 1:N | `AdoptionApplications.AdopterId` | без каскаду |
| AdoptionApplications → ApplicationDogs | 1:N | `ApplicationDogs.ApplicationId` | `Cascade` |
| AdoptionApplications → ApplicationStatusHistory | 1:N | `ApplicationStatusHistory.ApplicationId` | `Cascade` |
| AdoptionApplications ↔ AdoptionAgreements | 1:1 | `AdoptionAgreements.ApplicationId` (= PK) | `Cascade` |
| AdoptionApplications ↔ Requirements | M:N | `ApplicationRequirements (ApplicationId, RequirementId)`, поля `IsFulfilled`, `CheckedAt`, `CheckedBy`, `Notes` | `Cascade` / без каскаду |
| AdoptionApplications ↔ Dogs (локальна копія) | M:N | `ApplicationDogs (ApplicationId, DogId)`, поля `DogName`, `Priority` | `Cascade` / без каскаду |

### ERD
Діаграми побудовано на https://dbdiagram.io із файлів `docs/erd/*.dbml`:

![Каталог](docs/erd/catalog.png)
![Adoptions](docs/erd/adoptions.png)

## 7. Розгортання з нуля

Вимоги: SQL Server (наприклад, `localhost\SQLEXPRESS`), `sqlcmd`, MongoDB 7 (локально або Docker), `mongosh`, .NET SDK.

> **Кодування.** `.sql`-файли збережено в UTF-8 (без BOM). Завжди запускайте `sqlcmd` з прапором `-f 65001`, інакше українські дані в `seed.sql` потраплять у БД перекрученими. Прапор `-I` обов'язковий (QUOTED_IDENTIFIER для фільтрованих індексів).

### 7.0 (Опційно) СУБД у Docker
```powershell
docker compose up -d mongo
# SQL Server: створіть .env за зразком .env.example, потім
docker compose --profile sql up -d sqlserver
```

### 7.1 DogsService — Проєкт №2 (EF Core)
Рядок підключення: `DogsService.API/appsettings.json` → `ConnectionStrings:DogsDatabase`.

Package Manager Console (Default project: `DogsService.Infrastructure`, Startup: `DogsService.API`):
```powershell
Drop-Database -Project DogsService.Infrastructure -StartupProject DogsService.API      # лише для чистого розгортання
Update-Database -Project DogsService.Infrastructure -StartupProject DogsService.API
Script-Migration -Idempotent -Output database\catalog_schema.sql -Project DogsService.Infrastructure -StartupProject DogsService.API
```
Те саме через CLI: `dotnet ef database update -p DogsService.Infrastructure -s DogsService.API`.
Довідники (породи, вольєри, характеристики) та 5 собак із профілями застосовуються міграціями (`HasData`).

### 7.2 AdoptionsService — Проєкт №1 (SQL Server)
```powershell
cd AdoptionService\database
sqlcmd -S localhost\SQLEXPRESS -E -C -I -f 65001 -i schema.sql
sqlcmd -S localhost\SQLEXPRESS -E -C -I -f 65001 -i procedures.sql
sqlcmd -S localhost\SQLEXPRESS -E -C -I -f 65001 -i seed.sql
sqlcmd -S localhost\SQLEXPRESS -E -C -I -f 65001 -i queries.sql      # перевірка
# опційно — окремий логін із мінімальними правами (пароль передається змінною)
sqlcmd -S localhost\SQLEXPRESS -E -C -I -f 65001 -v AppPassword="<ваш_пароль>" -i security.sql
```
Для SQL-автентифікації замініть `-E` на `-U sa -P <пароль>`. Усі скрипти ідемпотентні: повторний запуск не створює дублікатів.

### 7.3 HealthService — Проєкт №3 (MongoDB)
```powershell
cd HealthService\database
mongosh "mongodb://localhost:27017" --file init.js
mongosh "mongodb://localhost:27017" --file seed.js
mongosh "mongodb://localhost:27017" --file queries.js
```
`seed.js` використовує `replaceOne(..., {upsert: true})` за фіксованими `_id` — повторний запуск дублікатів не створює.

## 8. Запуск API

| Сервіс | Команда | Swagger |
|---|---|---|
| DogsService | `dotnet run --project DogsService.API --urls http://localhost:5001` | `http://localhost:5001/swagger` |
| AdoptionsService | `dotnet run --project AdoptionService\AdoptionsService.API --urls http://localhost:5002` | `http://localhost:5002/swagger` |
| HealthService | `dotnet run --project HealthService\HealthService.API --urls http://localhost:5003` | `http://localhost:5003/swagger` |

Рядки підключення: `ConnectionStrings:DogsDatabase`, `ConnectionStrings:AdoptionsDatabase`, `MongoDb:ConnectionString` (можна перевизначити змінними оточення, наприклад `ConnectionStrings__AdoptionsDatabase`).

**Приклад сценарію (Adoptions):**
1. `GET /api/applications?status=Approved` — схвалена заявка зі seed.
2. `POST /api/applications/{id}/agreement` `{ "dogId": "d0000000-0000-0000-0000-000000000002", "agreementNumber": "AGR-2026-0001", "signedBy": "Андрій Мельник" }` — заявка стає `Completed`.
3. `POST /api/applications/{id}/status` `{ "newStatus": "Cancelled" }` — `409 Conflict` (Invalid transition: Completed -> Cancelled).

## 9. AdoptionsDb: процедури і статуси

- **Процедури (6):** `usp_CreateAdopter`, `usp_CreateApplication`, `usp_AddDogToApplication`, `usp_ChangeApplicationStatus` (явна транзакція, `UPDLOCK`, перевірка переходу), `usp_SignAgreement`, `usp_GetApplicationDetails`.
- **Статуси:** `Submitted → UnderReview → Approved → Completed` (`Completed` лише через `usp_SignAgreement`); `Rejected` / `Cancelled` — до `Completed`.
- **Помилки (THROW):** 50001–50003 не знайдено, 50004 недопустимий перехід, 50005 не виконано обов'язкові вимоги, 50006–50010 порушення правил операції, 50020 дублікат email.
- **Конкурентність:** `RowVersion` + `UPDLOCK, HOLDLOCK` у процедурах.

## 10. Індекси (під який запит)

| Індекс | Запит |
|---|---|
| `IX_Dogs_Name_Active` (unique, filtered) | перевірка дубліката імені при створенні собаки |
| `IX_Dogs_BreedId_Status_Active` | список собак породи за статусом |
| `IX_Dogs_KennelId_Active` | собаки у вольєрі |
| `IX_Dogs_Status_Active` | усі доступні для прилаштування |
| `IX_DogFeatures_FeatureId` | усі собаки з характеристикою X |
| `UX_Adopters_Email_Active` | пошук / унікальність заявника за email |
| `IX_Applications_AdopterId_Status` | заявки заявника за статусом |
| `IX_Applications_Status_SubmittedAt` | черга розгляду з пагінацією |
| `IX_ApplicationDogs_DogId` | заявки, де фігурує собака |
| `IX_StatusHistory_ApplicationId_ChangedAt` | історія статусів заявки |
| `IX_AppReq_Unfulfilled` (filtered) | невиконані вимоги |
| `IX_Dogs_Status` (Adoptions) | доступні собаки в локальній копії |
| Mongo `{dogId:1, visitDate:-1}` | візити собаки за часом |
| Mongo `{dogId:1, vaccinatedAt:-1}` | історія вакцинацій |
| Mongo `{nextDueAt:1}` (partial) | прострочені вакцинації |
| Mongo `{visitType:1, visitDate:-1}` | статистика за типом візиту |
| Mongo `{symptoms:1}` (multikey) | пошук за симптомом |

## 11. MongoDB: які прийоми де показано

| Прийом | Де |
|---|---|
| Embedding | `medicalRecords.allergies[]`, `chronicConditions[]`, `notes[]`; `vaccinations.vaccines[]` |
| Reference | `vaccinations.medicalRecordId`, `veterinaryVisits.medicalRecordId`, `relatedVisitId`; `dogId` → DogsDb (без FK) |
| Гібрид | візит: `medicalRecordId` (reference) + вбудований `dogSnapshot` (знімок) |
| Гнучка схема | `veterinaryVisits`: Checkup (`weightKg`, `temperatureC`), Surgery (`anesthesia`, `operationNotes`), Emergency (`triageLevel`), FollowUp (`relatedVisitId`) |
| Денормалізація | `dogName` у всіх колекціях |
| Validation / Aggregation | `init.js` (`$jsonSchema`), `queries.js` (`$group`, `$lookup`, `$unwind`) |
