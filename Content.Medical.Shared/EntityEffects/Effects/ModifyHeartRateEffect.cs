using Content.Shared.EntityEffects;

namespace Content.Medical.Shared.EntityEffects.Effects;

public sealed partial class ModifyHeartRateEffect : EntityEffectBase<ModifyHeartRateEffect>
{
    /// <summary>
    /// How much to change by.
    /// </summary>
    [DataField(required: true)]
    public float Amount;

    /// <summary>
    /// Should we multiply by <see cref="Amount"/> adds if false.
    /// </summary>
    [DataField]
    public bool Multiply;

    /// <summary>
    /// If a choom is flatlined should we still apply this effect.
    /// </summary>
    [DataField]
    public bool IgnoreFlatLine;

    /// <summary>
    /// If not null this effect will not raise above this Bpm;
    /// </summary>
    [DataField]
    public float? HigherLimit;

    /// <summary>
    /// If not null this effect will not lower the Bpm below this (although 0 will always be a hard limit);
    /// </summary>
    [DataField]
    public float? LowerLimit;
}
