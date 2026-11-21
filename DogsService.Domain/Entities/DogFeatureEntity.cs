using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DogsService.Domain.Entities;
public class DogFeatureEntity
{
    public Guid DogId { get; set; }

    public Guid FeatureId { get; set; }

    public DateTime AssignedAt { get; set; }

    public string? Notes { get; set; }

    // Navigation properties

    public DogEntity Dog { get; set; } = null!;

    public FeatureEntity Feature { get; set; } = null!;
}
