using HealthService.Domain.Entities;
using HealthService.Domain.Exceptions;
using HealthService.Domain.Interfaces;
using HealthService.Infrastructure.Mongo;
using MongoDB.Driver;

namespace HealthService.Infrastructure.Repositories;

public class MedicalRecordRepository(MongoDbContext _context) : IMedicalRecordRepository
{
    public async Task CreateAsync(MedicalRecord record, CancellationToken ct)
    {
        try
        {
            await _context.MedicalRecords.InsertOneAsync(record, cancellationToken: ct);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            // ux_medicalRecords_dogId
            throw new BusinessRuleException($"A medical record for dog '{record.DogId}' already exists.");
        }
    }

    public async Task<MedicalRecord?> GetByDogIdAsync(Guid dogId, CancellationToken ct) =>
        await _context.MedicalRecords.Find(r => r.DogId == dogId).FirstOrDefaultAsync(ct);

    public async Task<bool> AddAllergyAsync(Guid dogId, Allergy allergy, CancellationToken ct)
    {
        var update = Builders<MedicalRecord>.Update
            .Push(r => r.Allergies, allergy)
            .Set(r => r.UpdatedAt, DateTime.UtcNow);

        var result = await _context.MedicalRecords.UpdateOneAsync(r => r.DogId == dogId, update, cancellationToken: ct);
        return result.MatchedCount > 0;
    }

    public async Task<bool> AddNoteAsync(Guid dogId, MedicalNote note, CancellationToken ct)
    {
        var update = Builders<MedicalRecord>.Update
            .Push(r => r.Notes, note)
            .Set(r => r.UpdatedAt, DateTime.UtcNow);

        var result = await _context.MedicalRecords.UpdateOneAsync(r => r.DogId == dogId, update, cancellationToken: ct);
        return result.MatchedCount > 0;
    }

    public async Task UpdateDogNameAsync(Guid dogId, string dogName, CancellationToken ct)
    {
        await _context.MedicalRecords.UpdateManyAsync(
            r => r.DogId == dogId,
            Builders<MedicalRecord>.Update.Set(r => r.DogName, dogName),
            cancellationToken: ct);
    }
}
