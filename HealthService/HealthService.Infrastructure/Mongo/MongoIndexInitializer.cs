using HealthService.Domain.Entities;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HealthService.Infrastructure.Mongo;

/// <summary>
/// Створює індекси під реальні запити через Indexes.CreateManyAsync. Повторне створення безпечне:
/// імена й параметри збігаються з database/init.js (Лаб. 01), тож наявні індекси не конфліктують.
/// </summary>
public class MongoIndexInitializer(MongoDbContext context, ILogger<MongoIndexInitializer> logger)
{
    public async Task EnsureAsync(CancellationToken ct)
    {
        // Одна картка на собаку (аналог «один відгук від користувача на товар»)
        await context.MedicalRecords.Indexes.CreateManyAsync(
        [
            new CreateIndexModel<MedicalRecord>(
                Builders<MedicalRecord>.IndexKeys.Ascending(r => r.DogId),
                new CreateIndexOptions { Name = "ux_medicalRecords_dogId", Unique = true })
        ], ct);

        await context.Vaccinations.Indexes.CreateManyAsync(
        [
            // історія вакцинацій собаки (нові першими)
            new CreateIndexModel<Vaccination>(
                Builders<Vaccination>.IndexKeys.Ascending(v => v.DogId).Descending(v => v.VaccinatedAt),
                new CreateIndexOptions { Name = "ix_vaccinations_dogId_vaccinatedAt" }),

            // aggregation «прострочені вакцинації»; partial — лише документи з nextDueAt
            new CreateIndexModel<Vaccination>(
                Builders<Vaccination>.IndexKeys.Ascending(v => v.NextDueAt),
                new CreateIndexOptions<Vaccination>
                {
                    Name = "ix_vaccinations_nextDueAt",
                    PartialFilterExpression = new BsonDocument("nextDueAt", new BsonDocument("$exists", true))
                })
        ], ct);

        await context.VeterinaryVisits.Indexes.CreateManyAsync(
        [
            // візити собаки за часом: складений { dogId: 1, visitDate: -1 }
            new CreateIndexModel<VeterinaryVisit>(
                Builders<VeterinaryVisit>.IndexKeys.Ascending(v => v.DogId).Descending(v => v.VisitDate),
                new CreateIndexOptions { Name = "ix_visits_dogId_visitDate" }),

            // статистика/фільтр за типом візиту
            new CreateIndexModel<VeterinaryVisit>(
                Builders<VeterinaryVisit>.IndexKeys.Ascending(v => v.VisitType).Descending(v => v.VisitDate),
                new CreateIndexOptions { Name = "ix_visits_type_date" }),

            // пошук за симптомом (multikey по масиву)
            new CreateIndexModel<VeterinaryVisit>(
                Builders<VeterinaryVisit>.IndexKeys.Ascending("symptoms"),
                new CreateIndexOptions { Name = "ix_visits_symptoms" }),

            // повнотекстовий пошук за діагнозом і нотатками операції ($text)
            new CreateIndexModel<VeterinaryVisit>(
                Builders<VeterinaryVisit>.IndexKeys.Text("diagnosis").Text("operationNotes"),
                new CreateIndexOptions { Name = "tx_visits_diagnosis" }),

            // унікальність: той самий тип візиту для собаки в той самий момент — дублікат (→ 409)
            new CreateIndexModel<VeterinaryVisit>(
                Builders<VeterinaryVisit>.IndexKeys
                    .Ascending(v => v.DogId).Ascending(v => v.VisitDate).Ascending(v => v.VisitType),
                new CreateIndexOptions { Name = "ux_visits_dog_date_type", Unique = true })
        ], ct);

        logger.LogInformation("MongoDB indexes are in place.");
    }
}
