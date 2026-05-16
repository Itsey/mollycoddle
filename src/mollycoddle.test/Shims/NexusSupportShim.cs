namespace mollycoddle.test;

using Plisky.Diagnostics;

internal class NexusSupportShim : NexusSupport {

    public NexusSupportShim(MollyOptions mo) : base(mo) {
    }

    public Bilge GetBilge() => b;
}