using Content.Medical.Shared.Heart;
using Content.Shared.Body;

namespace Content.Medical.Shared.Body;

/// <summary>
/// Handles all the organ relays for larpmed
/// </summary>
public sealed partial class LarpMedBodySystem
{
    private void InitializeRelay()
    {
        SubscribeLocalEvent<BodyComponent, GetHeartsEvent>(RefRelayBodyEvent);
    }

    private void RefRelayBodyEvent<T>(EntityUid uid, BodyComponent component, ref T args) where T : struct
    {
        _body.RelayEvent((uid, component), ref args);
    }
}
