using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DogsService.Domain.Entities;
public class KennelEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public int Capacity { get; set; }

    // Navigation properties

    public ICollection<DogEntity> Dogs { get; set; }
        = new List<DogEntity>();
}
