using HealthService.Application.Abstractions;
using HealthService.Application.DTOs;
using HealthService.Domain.Enums;
using HealthService.Infrastructure.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HealthService.Infrastructure.Queries;

public class VisitQueries(MongoDbContext context) : IVisitQueries
{
    private IMongoCollection<BsonDocument> Visits => context.Raw(CollectionNames.VeterinaryVisits);

    public Task<VisitDto?> GetByIdAsync(ObjectId id, CancellationToken ct) =>
        MongoGuard.RunAsync(async () =>
        {
            var doc = await Visits.Find(Builders<BsonDocument>.Filter.Eq("_id", id)).FirstOrDefaultAsync(ct);
            return doc is null ? null : DocumentMapper.Visit(doc);
        });

    public Task<PagedResult<VisitListItemDto>> GetListAsync(
        Guid? dogId, VisitType? type, string? search, int page, int pageSize, CancellationToken ct) =>
        MongoGuard.RunAsync(async () =>
        {
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Empty;

            if (dogId.HasValue)
                criteria &= filter.Eq("dogId", dogId.Value.ToString());          // Guid зберігається рядком

            if (type.HasValue)
                criteria &= filter.Eq("visitType", type.Value.ToString());

            if (!string.IsNullOrWhiteSpace(search))
                criteria &= filter.Text(search);                                  // текстовий індекс tx_visits_diagnosis

            // проєкція: у списку не потрібні масиви, знімок, анестезія тощо
            var projection = new BsonDocument
            {
                { "dogId", 1 }, { "dogName", 1 }, { "visitDate", 1 }, { "visitType", 1 },
                { "veterinarian.name", 1 }, { "diagnosis", 1 }, { "triageLevel", 1 }, { "version", 1 }
            };

            // $match → $sort → $skip → $limit → $project: база повертає лише сторінку
            var items = await Visits.Aggregate()
                .Match(criteria)
                .Sort(Builders<BsonDocument>.Sort.Descending("visitDate").Descending("_id"))
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .Project<BsonDocument>(projection)
                .ToListAsync(ct);

            var total = await Visits.CountDocumentsAsync(criteria, cancellationToken: ct);

            return PagedResult<VisitListItemDto>.Create(
                items.Select(DocumentMapper.VisitListItem).ToList(), total, page, pageSize);
        });
}
