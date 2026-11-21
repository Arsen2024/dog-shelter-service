namespace DogsService.Application.DTOs.Dog;

public class DogDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Color { get; set; } = null!;
    public int TailLength { get; set; }
    public decimal Weight { get; set; }
    public string Status { get; set; } = null!;
    public Guid BreedId { get; set; }
    public string? BreedName { get; set; }
    public Guid? KennelId { get; set; }
    public string? KennelName { get; set; }
}
