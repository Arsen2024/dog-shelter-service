namespace DogsService.Infrastructure.Configurations;

/// <summary>
/// Фіксовані ідентифікатори seed-даних. Ті самі Id використовуються
/// в AdoptionsService (локальна копія Dogs) та HealthService (dogId).
/// </summary>
public static class SeedIds
{
    public static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    // Breeds
    public static readonly Guid Labrador = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid GermanShepherd = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid Beagle = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid Mixed = Guid.Parse("44444444-4444-4444-4444-444444444444");

    // Kennels
    public static readonly Guid KennelA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid KennelB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid KennelC = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    // Features
    public static readonly Guid Friendly = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    public static readonly Guid Trained = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
    public static readonly Guid GoodWithChildren = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
    public static readonly Guid Active = Guid.Parse("12345678-1234-1234-1234-123456789012");

    // Dogs
    public static readonly Guid Rex = Guid.Parse("d0000000-0000-0000-0000-000000000001");
    public static readonly Guid Bella = Guid.Parse("d0000000-0000-0000-0000-000000000002");
    public static readonly Guid Max = Guid.Parse("d0000000-0000-0000-0000-000000000003");
    public static readonly Guid Luna = Guid.Parse("d0000000-0000-0000-0000-000000000004");
    public static readonly Guid Charlie = Guid.Parse("d0000000-0000-0000-0000-000000000005");
}
