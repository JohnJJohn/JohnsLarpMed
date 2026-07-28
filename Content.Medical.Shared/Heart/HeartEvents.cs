using Content.Shared.Body;

namespace Content.Medical.Shared.Heart;

/// <summary>
/// Is relayed to each organ with the hearts adding themselves.
/// </summary>
/// <param name="Hearts">A list of every heart this entity has</param>
[ByRefEvent]
public record struct GetHeartsEvent(List<Entity<HeartComponent>> Hearts);

/// <summary>
/// Raised on a heart to apply any heart rate based effects that it might have.
/// </summary>
/// <param name="Organ">The organ component attached to this heart</param>
[ByRefEvent]
public record struct DoHeartEffectsEvent(OrganComponent Organ);
