namespace WebApp.Tests.Helpers;

/// <summary>
/// Immutable record of IDs created during a single SeedData call.
/// Stored as an instance field on CustomWebApplicationFactory — never static.
/// </summary>
public sealed record SeedResult(Guid TestUserId, Guid SecondUserId, Guid TestEquipmentId, Guid TestEquipmentCategoryId);
