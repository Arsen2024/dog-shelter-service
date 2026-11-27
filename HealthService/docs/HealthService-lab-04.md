# HealthService — Лаб. 04: Clean Architecture, CQRS (MediatR), MongoDB

Мікросервіс «Медичні картки» (Проєкт №3 з Лаб. 01, MongoDB): чотири шари з правильним напрямком залежностей,
сутності й Value Objects з інкапсуляцією, команди й запити MediatR, наскрізна валідація через Pipeline Behaviors,
інфраструктура MongoDB (BsonClassMap, індекси, seeding, health check) і Web/API з ProblemDetails.

## 1. Схема шарів

| Шар за методичкою | Проєкт | Призначення | Залежить від |
|---|---|---|---|
| Domain | `HealthService.Domain` | сутності (`BaseEntity`), Value Objects, винятки, інтерфейси репозиторіїв | **нічого** (лише `MongoDB.Bson` для `ObjectId`) |
| Application | `HealthService.Application` | команди, запити, обробники, behaviors, валідатори, DTO, AutoMapper; визначає `IVisitQueries` тощо | Domain |
| Infrastructure | `HealthService.Infrastructure` | MongoDB: контекст, BsonClassMap, репозиторії, запити з проєкціями, індекси, seeding, health check | Application, Domain |
| WebApi | `HealthService.API` | контролери (лише `ISender`), глобальний обробник помилок, реєстрація залежностей | Application, Infrastructure (лише для `AddInfrastructure`) |

```
WebApi ──► Application ──► Domain
   │            ▲              ▲
   └──► Infrastructure ────────┘     (зовнішні шари знають про внутрішні, не навпаки)
```

Кожен шар реєструє свої залежності методом розширення: `AddApplication()`, `AddInfrastructure(configuration)`.

## 2. Пакети

| Шар | Пакети |
|---|---|
| Domain | `MongoDB.Bson` (лише `ObjectId`) |
| Application | `MediatR` 12.4.1, `FluentValidation`, `FluentValidation.DependencyInjectionExtensions`, `AutoMapper` 14.0.0 |
| Infrastructure | `MongoDB.Driver`, `Microsoft.Extensions.Options.ConfigurationExtensions`, health checks / hosting abstractions |
| WebApi | `Swashbuckle.AspNetCore` |

MediatR 12.4.1 і AutoMapper 14.0.0 — останні версії **без** обов'язкового ліцензійного Community-ключа
(MediatR 13+ та AutoMapper 15+ його потребують).

## 3. Домен (Part 2)

- `BaseEntity`: `Id` (`ObjectId`), `CreatedAt`, `UpdatedAt`, `Version` (оптимістична конкурентність); setter-и закриті.
- Сутності: `VeterinaryVisit` (головний агрегат, аналог Review), `MedicalRecord`, `Vaccination`.
  Стан змінюють лише публічні методи (`Create`, `Update`, `AddAllergy`, `AddNote`, `RenameDog`, ...), що кидають `DomainException`.
- `ValueObject` — базовий клас з порівнянням за значеннями (як у eShop, `Ordering.Domain/SeedWork`).
  Незмінні Value Objects з перевіркою в конструкторі: `Veterinarian` (аналог Author), `Money`, `Email`, `DogSnapshot`,
  `Anesthesia`, `Allergy`, `ChronicCondition`, `MedicalNote`, `VaccineEntry` — вбудовуються як вкладені документи й масиви.
- Гнучка схема: у `veterinaryVisits` набір полів залежить від типу візиту (Checkup / Surgery / Emergency / FollowUp).
- BSON-атрибутів у домені немає — мапінг налаштовується в Infrastructure.
- Інтерфейси запису (`IVisitRepository`, `IMedicalRecordRepository`, `IVaccinationRepository`) оголошені в Domain.

## 4. CQRS і Pipeline Behaviors (Parts 3–4)

- Маркерні інтерфейси `ICommand`, `ICommand<T>`, `IQuery<T>`.
- Команди: `CreateVisitCommand`, `UpdateVisitCommand`, `DeleteVisitCommand`, `CreateMedicalRecordCommand`,
  `AddAllergyCommand`, `AddMedicalNoteCommand`, `RecordVaccinationCommand`, `RenameDogCommand`.
- Запити: `GetVisitByIdQuery`, `GetVisitsListQuery`, `GetMedicalRecordByDogIdQuery`, `GetVaccinationsByDogIdQuery`,
  `GetOverdueVaccinationsQuery`. Обробники запитів звертаються лише до `IVisitQueries` / `IMedicalRecordQueries` /
  `IVaccinationQueries`; жоден контролер чи обробник не бачить `IMongoCollection`.
- Порядок behaviors (від зовнішнього до внутрішнього) визначається порядком `AddOpenBehavior`:

```
Controller → ISender.Send
   ExceptionHandlingBehavior  (логує винятки на рівні MediatR)
   └ LoggingBehavior          (логує запит і відповідь)
     └ PerformanceBehavior    (попереджає про обробники > 500 мс)
       └ ValidationBehavior   (FluentValidation; помилки збираються разом)
         └ Handler            (не викликається, якщо валідація не пройшла)
```

- Валідатори є для всіх команд і критичних запитів, зокрема перевірка коректності `ObjectId` (`MustBeObjectId`).
- **Асинхронне правило з запитом до MongoDB:** `CreateMedicalRecordCommandValidator` перевіряє, що картки для цієї собаки ще немає
  (аналог «користувач ще не залишав відгук на товар»).

## 5. Інфраструктура MongoDB (Part 5, Part 8)

- `MongoDbContext` отримує налаштування через `IOptions<MongoDbSettings>` (секція `MongoDb`).
- `MongoMappings` (один раз при старті): конвенції camelCase / `IgnoreIfNull` / enum як рядок;
  приватні поля `_allergies`, `_chronicConditions`, `_notes`, `_vaccines`, `_symptoms`, `_recommendations` — через `MapField`;
  незмінні Value Objects створюються через конструктор у спеціальних серіалізаторах (`ValueObjectSerializers.cs`): `MapCreator` вимагає всіх аргументів конструктора в документі, а в документах Лаб. 01 необов'язкових полів (email, weightKg, notedAt, ...) немає.
- `VisitRepository` / `MedicalRecordRepository` / `VaccinationRepository` — запис. `UpdateAsync` виконує `ReplaceOne`
  лише для документа з очікуваною `version` (`{ _id, version }`); інакше `ConflictException`.
- `VisitQueries` / `MedicalRecordQueries` / `VaccinationQueries` — читання: `$match` → `$sort` → `$skip` → `$limit` → `$project`,
  `$text`, агрегація «прострочені вакцинації»; результат одразу мапиться в DTO без матеріалізації сутностей.
- **Індекси** створює фоновий сервіс при старті (`Indexes.CreateManyAsync`; повторне створення безпечне — імена й параметри
  збігаються з `database/init.js` з Лаб. 01):

| Колекція | Індекс | Під який запит |
|---|---|---|
| medicalRecords | `ux_medicalRecords_dogId` — унікальний `{ dogId }` | одна картка на собаку (аналог «один відгук від користувача на товар») |
| vaccinations | `ix_vaccinations_dogId_vaccinatedAt` — `{ dogId: 1, vaccinatedAt: -1 }` | історія вакцинацій собаки |
| vaccinations | `ix_vaccinations_nextDueAt` — partial | прострочені вакцинації |
| veterinaryVisits | `ix_visits_dogId_visitDate` — `{ dogId: 1, visitDate: -1 }` | список візитів собаки |
| veterinaryVisits | `ix_visits_type_date` — `{ visitType: 1, visitDate: -1 }` | фільтр за типом |
| veterinaryVisits | `ix_visits_symptoms` — multikey | пошук за симптомом |
| veterinaryVisits | `tx_visits_diagnosis` — текстовий | `search` у списку візитів |
| veterinaryVisits | `ux_visits_dog_date_type` — унікальний `{ dogId, visitDate, visitType }` | захист від дубліката візиту |

- **Seeding:** `IDataSeeder` для кожної колекції (`MedicalRecordSeeder`, `VaccinationSeeder`, `VisitSeeder`):
  якщо колекція порожня — одна `InsertManyAsync`; дані — JSON (Mongo Extended JSON, вбудований ресурс).
  Увімкнено `MongoDb:SeedOnStartup` у `appsettings.Development.json`, вимкнено в Production.
- **Health check** `GET /health` (ping до MongoDB). Пул і таймаути — у рядку підключення
  (`maxPoolSize=100&serverSelectionTimeoutMS=5000`).

## 6. Web/API і помилки (Parts 6–7)

| Метод і маршрут | Опис | Статуси |
|---|---|---|
| `GET /api/visits?dogId=&type=&search=&page=&pageSize=` | список візитів (пагінація, `$text`) | 200 / 400 |
| `GET /api/visits/by-dog/{dogId}` | візити собаки | 200 |
| `GET /api/visits/{id}` | візит (з `version`) | 200 / 400 / 404 |
| `POST /api/visits` | створення візиту | 201 + Location / 400 / 404 / 409 |
| `PUT /api/visits/{id}` | оновлення з `version` | 200 / 400 / 404 / 409 |
| `DELETE /api/visits/{id}` | видалення | 204 / 400 / 404 |
| `POST /api/medical-records`, `GET /api/medical-records/by-dog/{dogId}` | картка собаки | 201 / 200 / 400 / 404 |
| `POST /api/medical-records/by-dog/{dogId}/allergies`, `.../notes` | вбудовані масиви | 204 / 400 / 404 |
| `POST /api/vaccinations`, `GET /api/vaccinations/by-dog/{dogId}`, `GET /api/vaccinations/overdue` | вакцинації | 201 / 200 |
| `PUT /api/internal/dogs/{dogId}/name` | імітація події DogUpdated | 204 |
| `GET /health` | health check MongoDB | 200 / 503 |

Глобальний `IExceptionHandler` повертає `ProblemDetails` з `traceId`:

| Виняток | HTTP |
|---|---|
| `NotFoundException` | 404 |
| `ValidationException` (FluentValidation) | 400 (`ValidationProblemDetails` з помилками по полях) |
| `DomainException` | 400 |
| `ConflictException` (у т. ч. конфлікт версій) | 409 |
| `MongoWriteException` з `DuplicateKey` | 409 — шар Infrastructure (`MongoGuard`) перетворює на `ConflictException` |
| `MongoConnectionException` / таймаут вибору сервера | 503 — перетворюється на `ServiceUnavailableException` |
| решта | 500 (деталі лише в Development) |

MongoDB-винятки перекладає `MongoGuard` в Infrastructure, тож WebApi не залежить від драйвера.
Кожен виняток логується один раз: якщо його вже залогував `ExceptionHandlingBehavior`, глобальний обробник пропускає запис.

## 7. Запуск

```powershell
docker compose up -d mongo                       # якщо MongoDB ще не запущена
dotnet run --project HealthService\HealthService.API
```
Swagger — адреса з `launchSettings.json` + `/swagger`. У `Development` при старті створюються індекси й
наповнюються порожні колекції. Якщо база вже наповнена скриптами Лаб. 01 (`database/seed.js`), дані не дублюються.

Чисте розгортання: `docker exec dogsservice-mongo-1 mongosh --eval "db.getSiblingDB('HealthDb').dropDatabase()"` і запуск застосунку.

## 8. Демонстрація

| Сценарій | Запит | Очікування |
|---|---|---|
| Валідація до обробника (аналог Rating = 10) | `POST /api/visits`, `visitType: "Emergency"`, `triageLevel: 10` | `400` з помилками по полях; у лозі є `Handling CreateVisitCommand`, далі `rejected by validation, the handler was NOT executed`, рядка `Handled` **немає** |
| DomainException | `POST /api/visits`, `visitType: "Checkup"`, `costAmount: 5000000` | `400` (обробник виконувався, `Money` кидає `DomainException`) |
| Конфлікт версій | `GET /api/visits/{id}` → `version: 0`; `PUT` з `version: 0` → `200` (версія 1); повторний `PUT` з `version: 0` | `409` |
| Дублікат (унікальний індекс) | двічі `POST /api/visits` з однаковими `dogId`, `visitDate`, `visitType` | `201`, потім `409` |
| Асинхронна валідація | `POST /api/medical-records` з `dogId` існуючої картки | `400` («already exists») |
| Некоректний ObjectId | `GET /api/visits/abc` | `400` |
| 404 | `GET /api/visits/65f2ffffffffffffffffffff` | `404` |
| 503 | `docker stop dogsservice-mongo-1`, потім `GET /api/visits` | `503` приблизно через 5 с; `/health` повертає `Unhealthy`; `docker start dogsservice-mongo-1` |

Приклад тіла `POST /api/visits`:
```json
{
  "dogId": "d0000000-0000-0000-0000-000000000001",
  "visitDate": "2026-11-20T09:00:00Z",
  "visitType": "Checkup",
  "veterinarianName": "Dr. Petrenko",
  "veterinarianEmail": "petrenko@shelter.example",
  "symptoms": ["кашель"],
  "diagnosis": "Легка застуда",
  "recommendations": ["Тепло", "Спокій"],
  "dogBreed": "Labrador Retriever",
  "weightKg": 29.8,
  "temperatureC": 38.7,
  "costAmount": 450.00,
  "costCurrency": "UAH"
}
```

## 9. Що здається

- Git-репозиторій з чотирма проєктами; цей README зі схемою шарів і запуском;
- знімки екрана: Swagger; документ у колекції `veterinaryVisits` (MongoDB Compass) з camelCase і вкладеними `veterinarian`, `dogSnapshot`;
  результат індексів:
  `docker exec dogsservice-mongo-1 mongosh --quiet --eval "db.getSiblingDB('HealthDb').veterinaryVisits.getIndexes()"`
  (або `mongosh "mongodb://localhost:27017/HealthDb" --eval "db.veterinaryVisits.getIndexes()"`);
- демонстрація: помилка валідації до обробника (за логами) і конфлікт версій або дублікат візиту.
