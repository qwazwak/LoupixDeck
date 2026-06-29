namespace QPlug;

public static class RWLockSlimScopeExtensions
{
    extension(ReaderWriterLockSlim lockSlim)
    {
        public ReadScope EnterReadScope() => ReadScope.Enter(lockSlim);
        public WriteScope EnterWriteScope() => WriteScope.Enter(lockSlim);
        public UpgradeableReadScope EnterUpgradeableReadScope() => UpgradeableReadScope.Enter(lockSlim);
    }
}

public struct ReadScope : IDisposable
{
    private ReaderWriterLockSlim? lockSlim;
    private ReadScope(ReaderWriterLockSlim lockSlim) => this.lockSlim = lockSlim;
    public static ReadScope Enter(ReaderWriterLockSlim lockSlim)
    {
        ArgumentNullException.ThrowIfNull(lockSlim);
        lockSlim.EnterWriteLock();
        return new(lockSlim);
    }
    public void Dispose() => Interlocked.Exchange(ref lockSlim, null)?.ExitReadLock();
}

public struct WriteScope : IDisposable
{
    private ReaderWriterLockSlim? lockSlim;
    private WriteScope(ReaderWriterLockSlim lockSlim) => this.lockSlim = lockSlim;
    public static WriteScope Enter(ReaderWriterLockSlim lockSlim)
    {
        ArgumentNullException.ThrowIfNull(lockSlim);
        lockSlim.EnterWriteLock();
        return new(lockSlim);
    }
    public void Dispose() => Interlocked.Exchange(ref lockSlim, null)?.ExitReadLock();
}

public struct UpgradeableReadScope : IDisposable
{
    private ReaderWriterLockSlim? lockSlim;
    private UpgradeableReadScope(ReaderWriterLockSlim lockSlim) => this.lockSlim = lockSlim;
    public static UpgradeableReadScope Enter(ReaderWriterLockSlim lockSlim)
    {
        ArgumentNullException.ThrowIfNull(lockSlim);
        lockSlim.EnterWriteLock();
        return new(lockSlim);
    }
    public readonly WriteScope Upgrade()
    {
        ReaderWriterLockSlim? lockSlim = this.lockSlim;
        if (lockSlim is null)
            return default;
        return WriteScope.Enter(lockSlim);
    }
    public void Dispose() => Interlocked.Exchange(ref lockSlim, null)?.ExitReadLock();
}