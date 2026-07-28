using System.Linq;
using Content.Shared.Body;
using Content.Shared.Body.Events;
using Content.Shared.Damage.Systems;
using Content.Shared.EntityEffects;
using Content.Shared.Metabolism;
using Content.Shared.Mobs.Systems;
using JetBrains.Annotations;
using Robust.Shared.Timing;

namespace Content.Medical.Shared.Heart;

/// <summary>
/// Handles logic behind the <see cref="HeartComponent"/>
/// </summary>
public sealed partial class HeartSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SharedEntityEffectsSystem _entityEffects = default!;
    [Dependency] private MetabolizerSystem _metabolizer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyComponent, BleedModifierEvent>(OnBleedMod);
        SubscribeLocalEvent<HeartComponent, BodyRelayedEvent<GetHeartsEvent>>(OnGetHeartInfo);
        SubscribeLocalEvent<HeartComponent, OrganGotRemovedEvent>(OnHeartRemoved);
        SubscribeLocalEvent<HeartComponent, DoHeartEffectsEvent>(OnHeartUpdated);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HeartComponent, OrganComponent>();
        var curTime = _timing.CurTime;

        while (query.MoveNext(out var uid, out var heart, out var organ))
        {
            if (curTime > heart.EffectNextUpdate)
            {
                var ev = new DoHeartEffectsEvent(organ);
                RaiseLocalEvent(uid, ref ev);
            }

            if (curTime < heart.NextUpdate || organ.Body is not {} body || !_mobState.IsAlive(body))
                continue;

            if (!ShouldHeartBeUpdated((uid, heart), curTime, organ))
                continue;

            UpdateHeart(heart, curTime);
        }
    }

    /// <summary>
    /// Applies all the entity effects for each heart.
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="args"></param>
    private void OnHeartUpdated(Entity<HeartComponent> ent, ref DoHeartEffectsEvent args)
    {
        if (args.Organ.Body is null)
            return;

        var heartRate = ent.Comp.HeartRate;
        var body = args.Organ.Body.Value;

        ent.Comp.EffectNextUpdate = _timing.CurTime + ent.Comp.EffectsUpdateInterval;

        foreach (var (key, value) in ent.Comp.HeartRateEffectsHigher)
        {
            if (heartRate < key)
                continue;

            _entityEffects.ApplyEffects(body, value);
        }

        foreach (var (key, value) in ent.Comp.HeartRateEffectsLower)
        {
            if (heartRate > key)
                continue;

            _entityEffects.ApplyEffects(body, value);
        }
    }

    /// <summary>
    /// Updates the specified heart.
    /// </summary>
    /// <param name="heart"></param>
    /// <param name="curTime"></param>
    private void UpdateHeart(HeartComponent heart, TimeSpan curTime)
    {
        var diff = heart.TargetHeartRate - heart.HeartRate;
        var change = diff * 0.1f;

        // I facking hate maths
        change = MathF.Sign(change) * Math.Clamp(MathF.Abs(change), 0.5f, 100f);

        heart.HeartRate += change;

        if (diff >= 0)
            heart.HeartRate = Math.Min(heart.HeartRate, heart.TargetHeartRate);
        else
            heart.HeartRate = Math.Max(heart.HeartRate, heart.TargetHeartRate);

        heart.NextUpdate = curTime + heart.UpdateInterval;
    }

    private void OnHeartRemoved(Entity<HeartComponent> ent, ref OrganGotRemovedEvent args)
    {
        SetHeartRateOfHeart(ent, 0);
    }

    private void OnGetHeartInfo(Entity<HeartComponent> ent, ref BodyRelayedEvent<GetHeartsEvent> args)
    {
        var hearts = args.Args;
        hearts.Hearts.Add(ent);

        args.Args = hearts;
    }

    private void OnBleedMod(Entity<BodyComponent> ent, ref BleedModifierEvent args)
    {
        args.BleedAmount *= GetBleedModOfEntity(ent.Owner);
    }

    #region PublicAPI

    /// <summary>
    /// Sets the target heart rate of all the heart in an entity to the amount specified.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="setTo"></param>
    [PublicAPI]
    public void SetHeartRate(EntityUid uid, float setTo)
    {
        var hearts = GetHearts(uid);

        foreach (var heart in hearts)
        {
            SetHeartRateOfHeart(heart, setTo);
        }
    }

    /// <summary>
    /// Sets the target heart rate of a singular heart to the amount specified.
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="setTo"></param>
    [PublicAPI]
    public void SetHeartRateOfHeart(Entity<HeartComponent> ent, float setTo)
    {
        ent.Comp.TargetHeartRate = setTo;
    }

    /// <summary>
    /// Changes the target heart rate of a singular heart by the amount specified.
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="amount"></param>
    [PublicAPI]
    public void ChangeHeartRateOfHeart(Entity<HeartComponent> ent, float amount)
    {
        ent.Comp.TargetHeartRate += amount;
    }

    /// <summary>
    /// Multiplies the target heart rate of a singular heart by the amount specified.
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="amount"></param>
    [PublicAPI]
    public void MultiplyHeartRateOfHeart(Entity<HeartComponent> ent, float amount)
    {
        ent.Comp.TargetHeartRate *= amount;
    }

    /// <summary>
    /// Changes the target heart rate of all the heart in an entity by the amount specified.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="amount">Amount to change by</param>
    [PublicAPI]
    public void ChangeHeartRate(EntityUid uid, float amount)
    {
        var hearts = GetHearts(uid);

        foreach (var heart in hearts)
        {
            ChangeHeartRateOfHeart(heart, amount);
        }
    }

    /// <summary>
    /// Multiplies the target heart rate of all the heart in an entity by the amount specified.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="amount">Amount to change by</param>
    [PublicAPI]
    public void MultiplyHeartRate(EntityUid uid, float amount)
    {
        var hearts = GetHearts(uid);

        foreach (var heart in hearts)
        {
            MultiplyHeartRateOfHeart(heart, amount);
        }
    }

    [PublicAPI]
    public bool IsLimitPass(Entity<HeartComponent> heart, float upperLimit, float lowerLimit)
    {
        var heartRate = heart.Comp.HeartRate;

        if (heartRate <= lowerLimit || heartRate >= upperLimit)
            return false;

        return true;
    }

    /// <summary>
    /// Gets all the hearts of an entity, returns an empty list if it has none, or has no <see cref="BodyComponent"/>
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    [PublicAPI]
    public List<Entity<HeartComponent>> GetHearts(EntityUid uid)
    {
        var ev = new GetHeartsEvent(new());
        RaiseLocalEvent(uid, ref ev);

        return ev.Hearts;
    }

    /// <summary>
    /// Gets the current BPM of an entity
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    [PublicAPI]
    public float GetBpmFromEntity(EntityUid uid)
    {
        var hearts = GetHearts(uid);

        if (hearts.Count == 0)
            return 0;

        return GetBpmFromHearts(hearts);
    }

    /// <summary>
    /// Gets the combined total of BPM from each heart in the body of an entity.
    /// </summary>
    /// <param name="hearts">Hearts to check</param>
    /// <returns></returns>
    [PublicAPI]
    public float GetBpmFromHearts(List<Entity<HeartComponent>> hearts)
    {
        var bpm = 0f;

        foreach (var heart in hearts)
        {
            bpm += heart.Comp.HeartRate;
        }

        return bpm;
    }

    /// <summary>
    /// Returns the average of all the <see cref="HeartComponent.NormalHeartRate"/> of all the hearts inside an entity.
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    [PublicAPI]
    public float GetNormalBpmOfEntity(EntityUid uid)
    {
        var hearts = GetHearts(uid);

        var values = new List<float>();

        foreach (var heart in hearts)
        {
            values.Add(heart.Comp.NormalHeartRate);
        }

        if (values.Sum() == 0)
            return 1;

        return values.Sum() / values.Count;
    }

    /// <summary>
    /// Gets the bleed modifier from an entity.
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    [PublicAPI]
    public float GetBleedModOfEntity(EntityUid uid)
    {
        var heartRate = GetBpmFromEntity(uid);

        var ratio = heartRate / GetNormalBpmOfEntity(uid);
        return MathF.Pow(ratio, 2f);
    }

    /// <summary>
    /// Returns if a heart entity should be updated or not.
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="curTime"></param>
    /// <param name="organ"></param>
    /// <returns></returns>
    [PublicAPI]
    public bool ShouldHeartBeUpdated(Entity<HeartComponent> ent, TimeSpan curTime, OrganComponent organ)
    {
        if (Math.Abs(ent.Comp.TargetHeartRate - ent.Comp.HeartRate) < 0.01) // Makes sure that the absolute difference between them is less than 0.01 to prevent doing unneeded updates.
            return false;

        return true;
    }

    #endregion
}
