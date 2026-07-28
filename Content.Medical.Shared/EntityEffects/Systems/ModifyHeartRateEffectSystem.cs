using Content.Medical.Shared.EntityEffects.Effects;
using Content.Medical.Shared.Heart;
using Content.Shared.Body;
using Content.Shared.EntityEffects;

namespace Content.Medical.Shared.EntityEffects.Systems;

public sealed partial class ModifyHeartRateEffectSystem : EntityEffectSystem<BodyComponent, ModifyHeartRateEffect>
{
    [Dependency] private HeartSystem _heart = default!;

    protected override void Effect(Entity<BodyComponent> entity, ref EntityEffectEvent<ModifyHeartRateEffect> args)
    {
        var effect = args.Effect;

        var hearts = _heart.GetHearts(entity.Owner);

        foreach (var heart in hearts)
        {
            if (effect.HigherLimit is not null && effect.LowerLimit is not null && !_heart.IsLimitPass(heart, effect.HigherLimit.Value, effect.LowerLimit.Value))
                continue;

            if (heart.Comp.HeartRate == 0 && !effect.IgnoreFlatLine)
                continue;

            if (effect.Multiply)
                _heart.MultiplyHeartRate(entity.Owner, effect.Amount);
            else
                _heart.ChangeHeartRate(entity.Owner, effect.Amount);
        }
    }
}
