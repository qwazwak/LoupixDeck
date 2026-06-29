using Microsoft.Extensions.DependencyInjection;
using NAudio.CoreAudioApi;

namespace QPlug;

internal sealed class MetaDefaultDeviceReferencer(
    [FromKeyedServices(Role.Console)] DefaultDeviceReferencer console,
    [FromKeyedServices(Role.Multimedia)] DefaultDeviceReferencer multimedia,
    [FromKeyedServices(Role.Communications)] DefaultDeviceReferencer comm
    ) : IDisposable
{

    public void VolumeStepUp()
    {
        console?.VolumeStepUp();
        multimedia?.VolumeStepUp();
        comm?.VolumeStepUp();
    }

    public void VolumeStepDown()
    {
        console?.VolumeStepUp();
        multimedia?.VolumeStepUp();
        comm?.VolumeStepUp();
    }

    /*
    public void ToggleMute()
    {
        console?.ToggleMute();
        multimedia?.ToggleMute();
        comm?.ToggleMute();
    }
    */

    public void Dispose()
    {
        Interlocked.Exchange(ref console!, null)?.Dispose();
        Interlocked.Exchange(ref multimedia!, null)?.Dispose();
        Interlocked.Exchange(ref comm!, null)?.Dispose();
    }
}
