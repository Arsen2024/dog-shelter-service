using MongoDB.Bson;

namespace HealthService.Domain.Common;

/// <summary>
/// Базова сутність: Id (ObjectId), аудит і Version для оптимістичної конкурентності.
/// Setter-и закриті — стан змінюють лише публічні методи сутностей.
/// </summary>
public abstract class BaseEntity
{
    protected BaseEntity()
    {
        Id = ObjectId.GenerateNewId();
        CreatedAt = DateTime.UtcNow;
    }

    protected BaseEntity(ObjectId? id) : this()
    {
        if (id.HasValue)
            Id = id.Value;
    }

    public ObjectId Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>Версія документа: оновлення застосовується лише до документа з очікуваною версією.</summary>
    public long Version { get; protected set; }

    protected void Touch() => UpdatedAt = DateTime.UtcNow;

    /// <summary>Викликає репозиторій безпосередньо перед ReplaceOne з перевіркою версії.</summary>
    public void IncrementVersion() => Version++;
}
