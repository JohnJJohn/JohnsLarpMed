using Content.Shared.Body;

namespace Content.Medical.Shared.Body;

/// <summary>
/// The larpmed version of the <see cref="BodySystem"/>
/// </summary>
public partial class LarpMedBodySystem : EntitySystem
{
    [Dependency] private BodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeRelay();
    }
}
