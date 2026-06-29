using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using QCommon.Utils.Extensions.Logging;

namespace QPlug;

internal sealed class DefaultDeviceReferencer(ILogger<DefaultDeviceReferencer> log, [ServiceKey] Role role) : IDisposable, IMMNotificationClient
{
    private const DataFlow dataFlow = DataFlow.Render;
    private readonly Lock @lock = new();
    private readonly Role role = role;
    private MMDevice? currentDefaultDevice;
    private bool isDisposed;

    public void VolumeStepUp() => VolumeStep(static v => v.VolumeStepUp(), "up");
    public void VolumeStepDown() => VolumeStep(static v => v.VolumeStepDown(), "down");

    private void VolumeStep(Action<AudioEndpointVolume> doStep, string name)
    {
        using IDisposable? _ = log.BeginScopeTags([new(nameof(Role), role)]);
        float oldLevel;
        float newLevel;
        using (Lock.Scope scope = @lock.EnterScope())
        {
            if (isDisposed)
                return;
            AudioEndpointVolume? volEndpoint = currentDefaultDevice?.AudioEndpointVolume;
            if (volEndpoint is null)
                return;
            oldLevel = volEndpoint.MasterVolumeLevel;
            doStep.Invoke(volEndpoint);
            newLevel = volEndpoint.MasterVolumeLevel;
        }
        log.LogInformation("Volume stepped {name} from {oldLevel} to {newLevel} (diff of {diff})", name, oldLevel, newLevel, newLevel - oldLevel);
    }

    public void ToggleMute()
    {
        using IDisposable? _ = log.BeginScopeTags([new(nameof(Role), role)]);
        using Lock.Scope scope = @lock.EnterScope();
        AudioEndpointVolume? volumeControl = currentDefaultDevice?.AudioEndpointVolume;
        if (volumeControl is null)
            return;
        volumeControl.Mute = !volumeControl.Mute;
    }

    void IMMNotificationClient.OnDeviceStateChanged(string deviceId, DeviceState newState) { }
    void IMMNotificationClient.OnDeviceAdded(string pwstrDeviceId) { }
    void IMMNotificationClient.OnPropertyValueChanged(string deviceId, PropertyKey key) { }

    public void OnDeviceRemoved(string deviceId)
    {
        MMDevice? oldDevice;
        lock (@lock)
        {
            if (isDisposed)
                return;
            if ((currentDefaultDevice?.ID) != deviceId)
                return;
            oldDevice = Interlocked.Exchange(ref currentDefaultDevice, null);
        }
        oldDevice?.Dispose();
    }

    public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
    {
        if (isDisposed || flow != dataFlow || role != this.role)
            return;
        OnDefaultDeviceChanged(defaultDeviceId);
    }

    public void OnDefaultDeviceChanged(string defaultDeviceId)
    {
        MMDevice newDevice = GetDevice(defaultDeviceId);
        MMDevice? oldDevice;
        lock (@lock)
        {
            oldDevice = Interlocked.Exchange(ref currentDefaultDevice, newDevice);
        }
        oldDevice?.Dispose();
    }
    private static MMDevice GetDevice(string deviceId)
    {
        using MMDeviceEnumerator enumerator = new();
        return enumerator.GetDevice(deviceId);
    }

    private void ReplaceDevice(MMDevice? newDevice)
    {
        MMDevice? oldDevice;
        lock (@lock)
        {
            oldDevice = Interlocked.Exchange(ref currentDefaultDevice, newDevice);
        }
        oldDevice?.Dispose();
    }
    public void Dispose()
    {
        isDisposed = true;
        ReplaceDevice(null);
    }
}

public readonly struct VolumeRange(float minDecibels, float maxDecibels, float incrementDecibels)
{
    public readonly float MinDecibels = minDecibels;
    public readonly float MaxDecibels = maxDecibels;
    public readonly float IncrementDecibels = incrementDecibels;

    public float Clamp(float currentValue, float adjustment) => Clamp(currentValue + adjustment);
    public float Clamp(float newValue) => Math.Clamp(newValue, MinDecibels, MaxDecibels);

    public static implicit operator VolumeRange(AudioEndpointVolumeVolumeRange range) => new(range.MinDecibels, range.MaxDecibels, range.IncrementDecibels);
}