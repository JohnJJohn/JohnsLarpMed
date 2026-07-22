using Robust.Shared.ContentPack;

namespace Content.Medical.Client.Entry;

public sealed class EntryPoint : GameClient
{
    public override void Init()
    {
        Dependencies.BuildGraph();
        Dependencies.InjectDependencies(this);
    }
}
