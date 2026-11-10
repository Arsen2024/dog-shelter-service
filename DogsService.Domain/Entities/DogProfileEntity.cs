using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DogsService.Domain.Entities;
public class DogProfileEntity
{
    public Guid Id { get; set; }

    public Guid DogId { get; set; }

    public DateTime? BirthDate { get; set; }

    public string? Gender { get; set; }

    public string? Description { get; set; }

    public string? Temperament { get; set; }

    // Navigation property

    public DogEntity Dog { get; set; } = null!;
}
