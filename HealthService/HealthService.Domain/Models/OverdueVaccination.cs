namespace HealthService.Domain.Models;

public class OverdueVaccination
{
    public Guid DogId { get; set; }
    public string DogName { get; set; } = string.Empty;
    public DateTime LastVaccinatedAt { get; set; }
    public DateTime NextDueAt { get; set; }
}
