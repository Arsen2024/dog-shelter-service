using DogsService.Domain.Entities;
using DogsService.Domain.Enums;
using DogsService.Infrastructure.Configurations;
using DogsService.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DogsService.Infrastructure.Seeding;

/// <summary>
/// Програмний сідінг залежних даних після MigrateAsync(). Довідники (породи, вольєри, характеристики)
/// сіються статично через HasData. Сідінг ідемпотентний: кожна собака додається, лише якщо її ще немає
/// (з урахуванням soft-deleted записів).
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        // спершу довідники — вони мають існувати (створюються міграцією)
        if (!await db.Breeds.AnyAsync(ct) || !await db.Kennels.AnyAsync(ct) || !await db.Features.AnyAsync(ct))
        {
            logger.LogWarning("Reference data (breeds, kennels, features) is missing. Apply migrations before seeding.");
            return;
        }

        var added = 0;

        foreach (var dog in BuildDogs())
        {
            if (await db.Dogs.IgnoreQueryFilters().AnyAsync(d => d.Id == dog.Id, ct))
                continue;

            await db.Dogs.AddAsync(dog, ct);
            added++;
        }

        if (added == 0)
        {
            logger.LogInformation("Seeding skipped: all seed dogs already exist.");
            return;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded {Count} dogs with profiles and features.", added);
    }

    private static IEnumerable<DogEntity> BuildDogs()
    {
        var rex = new DogEntity("Rex", "golden", 35, 29.50m, SeedIds.Labrador, SeedIds.KennelA, SeedIds.Rex);
        rex.SetProfile(new DateTime(2022, 3, 14), "Male", "Energetic retriever, loves water.", "Playful, friendly");
        rex.AddFeature(SeedIds.Friendly, "Greets everyone", SeedIds.SeedDate);
        rex.AddFeature(SeedIds.Trained, "Basic commands", SeedIds.SeedDate.AddDays(10));

        var bella = new DogEntity("Bella", "black & tan", 40, 27.00m, SeedIds.GermanShepherd, SeedIds.KennelB, SeedIds.Bella);
        bella.SetProfile(new DateTime(2021, 7, 2), "Female", "Calm and attentive shepherd.", "Loyal, calm");
        bella.AddFeature(SeedIds.Trained, null, SeedIds.SeedDate.AddDays(20));
        bella.AddFeature(SeedIds.Active, "Needs long walks", SeedIds.SeedDate.AddDays(30));

        var max = new DogEntity("Max", "brown & white", 25, 12.30m, SeedIds.Beagle, SeedIds.KennelA, SeedIds.Max);
        max.SetProfile(new DateTime(2023, 1, 20), "Male", "Curious beagle with a great nose.", "Curious, social");
        max.AddFeature(SeedIds.Friendly, null, SeedIds.SeedDate.AddDays(40));
        max.AddFeature(SeedIds.GoodWithChildren, "Tested with kids", SeedIds.SeedDate.AddDays(50));
        max.ChangeStatus(DogStatus.Reserved);

        var luna = new DogEntity("Luna", "grey", 20, 9.80m, SeedIds.Mixed, SeedIds.KennelC, SeedIds.Luna);
        luna.SetProfile(new DateTime(2020, 11, 5), "Female", "Shy mixed-breed, recovering after surgery.", "Shy, gentle");
        luna.AddFeature(SeedIds.Friendly, null, SeedIds.SeedDate.AddDays(60));
        luna.ChangeStatus(DogStatus.UnderTreatment);

        var charlie = new DogEntity("Charlie", "black", 33, 31.20m, SeedIds.Labrador, null, SeedIds.Charlie);
        charlie.SetProfile(new DateTime(2019, 5, 30), "Male", "Adopted family dog.", "Calm, affectionate");
        charlie.AddFeature(SeedIds.GoodWithChildren, null, SeedIds.SeedDate.AddDays(70));
        charlie.ChangeStatus(DogStatus.Reserved);
        charlie.ChangeStatus(DogStatus.Adopted);

        return [rex, bella, max, luna, charlie];
    }
}
