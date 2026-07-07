namespace QCommon.Utils;

internal sealed class NullDisposable : IDisposable
{
    public static NullDisposable Instance = new();
    private NullDisposable() { }
    public void Dispose() { }
}
