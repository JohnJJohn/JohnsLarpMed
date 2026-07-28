using Content.Shared.EntityEffects;
using Robust.Shared.GameStates;

namespace Content.Medical.Shared.Heart;

/// <summary>
/// Attached the heart organ.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HeartComponent : Component
{
    /// <summary>
    /// The normal heart rate for this heart. <see cref="HeartRate"/> will try to stabilize to this every update.
    /// </summary>
    [DataField]
    public float NormalHeartRate = 60;

    /// <summary>
    /// The max heart rate before a heart attack is possible.
    /// </summary>
    [DataField]
    public float MaxHeartRate = 150f;

    /// <summary>
    /// The min heart rate before the person starts to take asphyxiation damage.
    /// </summary>
    [DataField]
    public float MinHeartRate = 20f;

    /// <summary>
    /// Chance to have a heart attack when over <see cref="MaxHeartRate"/>.
    /// </summary>
    [DataField]
    public float HeartAttackChance = 0.03f;

    /// <summary>
    /// The current heart rate of the heart in BPM.
    /// </summary>
    [ViewVariables]
    public float HeartRate = 60f;

    /// <summary>
    /// Each heart update <see cref="HeartRate"/> will move towards this
    /// </summary>
    [DataField]
    public float TargetHeartRate = 60f;

    [ViewVariables]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [ViewVariables]
    public TimeSpan EffectNextUpdate = TimeSpan.Zero;

    /// <summary>
    /// How often does this heart update.
    /// </summary>
    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(0.5);

    /// <summary>
    /// How often does this heart update effects.
    /// </summary>
    [DataField]
    public TimeSpan EffectsUpdateInterval = TimeSpan.FromSeconds(1.8);

    /// <summary>
    /// The effects to apply when the heartrate of an entity is higher than the key.
    /// </summary>
    [DataField]
    public Dictionary<float, EntityEffect[]> HeartRateEffectsHigher = new ();

    /// <summary>
    /// The effects to apply when the heartrate of an entity is lower than the key.
    /// </summary>
    [DataField]
    public Dictionary<float, EntityEffect[]> HeartRateEffectsLower = new ();
}
