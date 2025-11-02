namespace DogsService.Domain.Entities;

public class DogEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Color { get; set; } = null!;
    public int TailLength { get; set; }
    public int Weight { get; set; }
}