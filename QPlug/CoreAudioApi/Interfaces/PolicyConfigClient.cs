using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace CoreAudioApi.Interfaces;

internal sealed partial class PolicyConfigClient
{
    [ComImport, Guid("870af99c-171d-4f9e-af0d-e63df40c2bc9")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    private class _PolicyConfigClient;

    private readonly _PolicyConfigClient client = new();

    private static WaveFormatExtensible WrapWaveFormat<TState>(PolicyConfigClient self, TState state, Action<PolicyConfigClient, TState, IntPtr> func)
    {

        IntPtr formatPtr = Marshal.AllocHGlobal(Marshal.SizeOf<WaveFormatExtensible>());
        try
        {
            func.Invoke(self, state, formatPtr);
            WaveFormat result = WaveFormat.MarshalFromPtr(formatPtr);
            Debug.Assert(result is WaveFormatExtensible);
            return (WaveFormatExtensible)result;
        }
        finally
        {
            Marshal.FreeHGlobal(formatPtr);
        }
    }
    public WaveFormatExtensible GetMixFormat(string pszDeviceName)
        => WrapWaveFormat(this, pszDeviceName, static (self, pszDeviceName, formatPtr) =>
        {
            if (self.client is IPolicyConfig policyConfig)
                Marshal.ThrowExceptionForHR(policyConfig.GetMixFormat(pszDeviceName, formatPtr));
            else if (self.client is IPolicyConfigVista policyConfigVista)
                Marshal.ThrowExceptionForHR(policyConfigVista.GetMixFormat(pszDeviceName, formatPtr));
            else if (self.client is IPolicyConfig10 policyConfig10)
                Marshal.ThrowExceptionForHR(policyConfig10.GetMixFormat(pszDeviceName, formatPtr));
            else
                throw ExNone();
        });

    public WaveFormatExtensible GetDeviceFormat(string pszDeviceName, bool bDefault)
        => WrapWaveFormat(this, (pszDeviceName, bDefault), static (self, args, formatPtr) =>
        {
            if (self.client is IPolicyConfig policyConfig)
                Marshal.ThrowExceptionForHR(policyConfig.GetDeviceFormat(args.pszDeviceName, args.bDefault, formatPtr));
            else if (self.client is IPolicyConfigVista policyConfigVista)
                Marshal.ThrowExceptionForHR(policyConfigVista.GetDeviceFormat(args.pszDeviceName, args.bDefault, formatPtr));
            else if (self.client is IPolicyConfig10 policyConfig10)
                Marshal.ThrowExceptionForHR(policyConfig10.GetDeviceFormat(args.pszDeviceName, args.bDefault, formatPtr));
            else
                throw ExNone();
        });

    public void ResetDeviceFormat(string pszDeviceName)
    {
        if (client is IPolicyConfig policyConfig)
            Marshal.ThrowExceptionForHR(policyConfig.ResetDeviceFormat(pszDeviceName));
        else if (client is IPolicyConfigVista policyConfigVista)
            Marshal.ThrowExceptionForHR(policyConfigVista.ResetDeviceFormat(pszDeviceName));
        else if (client is IPolicyConfig10 policyConfig10)
            Marshal.ThrowExceptionForHR(policyConfig10.ResetDeviceFormat(pszDeviceName));
        else
            throw ExNone();
    }

#if false

    public void SetDeviceFormat(string pszDeviceName, nint pEndpointFormat, nint MixFormat)
    {
        if (client is IPolicyConfig policyConfig)
            Marshal.ThrowExceptionForHR(policyConfig.SetDeviceFormat(pszDeviceName, pEndpointFormat, MixFormat));
        else if (client is IPolicyConfigVista policyConfigVista)
            Marshal.ThrowExceptionForHR(policyConfigVista.SetDeviceFormat(pszDeviceName, pEndpointFormat, MixFormat));
        else if (client is IPolicyConfig10 policyConfig10)
            Marshal.ThrowExceptionForHR(policyConfig10.SetDeviceFormat(pszDeviceName, pEndpointFormat, MixFormat));
        else
            throw ExNone();
    }

    public void GetProcessingPeriod(string pszDeviceName, bool bDefault, nint pmftDefaultPeriod, nint pmftMinimumPeriod)
    {
        if (client is IPolicyConfig policyConfig)
            Marshal.ThrowExceptionForHR(policyConfig.GetProcessingPeriod(pszDeviceName, bDefault, pmftDefaultPeriod, pmftMinimumPeriod));
        else if (client is IPolicyConfigVista policyConfigVista)
            Marshal.ThrowExceptionForHR(policyConfigVista.GetProcessingPeriod(pszDeviceName, bDefault, pmftDefaultPeriod, pmftMinimumPeriod));
        else if (client is IPolicyConfig10 policyConfig10)
            Marshal.ThrowExceptionForHR(policyConfig10.GetProcessingPeriod(pszDeviceName, bDefault, pmftDefaultPeriod, pmftMinimumPeriod));
        else
            throw ExNone();
    }

    public void SetProcessingPeriod(string pszDeviceName, nint pmftPeriod)
    {
        if (client is IPolicyConfig policyConfig)
            Marshal.ThrowExceptionForHR(policyConfig.SetProcessingPeriod(pszDeviceName, pmftPeriod));
        else if (client is IPolicyConfigVista policyConfigVista)
            Marshal.ThrowExceptionForHR(policyConfigVista.SetProcessingPeriod(pszDeviceName, pmftPeriod));
        else if (client is IPolicyConfig10 policyConfig10)
            Marshal.ThrowExceptionForHR(policyConfig10.SetProcessingPeriod(pszDeviceName, pmftPeriod));
        else
            throw ExNone();
    }

    public AudioClientShareMode GetShareMode(string pszDeviceName)
    {
        AudioClientShareMode pMode;
        if (client is IPolicyConfig policyConfig)
            Marshal.ThrowExceptionForHR(policyConfig.GetShareMode(pszDeviceName, out pMode));
        else if (client is IPolicyConfigVista policyConfigVista)
            Marshal.ThrowExceptionForHR(policyConfigVista.GetShareMode(pszDeviceName, out pMode));
        else if (client is IPolicyConfig10 policyConfig10)
            Marshal.ThrowExceptionForHR(policyConfig10.GetShareMode(pszDeviceName, out pMode));
        else
            throw ExNone();
        return pMode;
    }

    public void SetShareMode(string pszDeviceName, AudioClientShareMode mode)
    {
        if (client is IPolicyConfig policyConfig)
            Marshal.ThrowExceptionForHR(policyConfig.SetShareMode(pszDeviceName, mode));
        else if (client is IPolicyConfigVista policyConfigVista)
            Marshal.ThrowExceptionForHR(policyConfigVista.SetShareMode(pszDeviceName, mode));
        else if (client is IPolicyConfig10 policyConfig10)
            Marshal.ThrowExceptionForHR(policyConfig10.SetShareMode(pszDeviceName, mode));
        else
            throw ExNone();
    }

    public void GetPropertyValue(string pszDeviceName, bool bFxStore, nint key, nint pv)
    {
        if (client is IPolicyConfig policyConfig)
            Marshal.ThrowExceptionForHR(policyConfig.GetPropertyValue(pszDeviceName, bFxStore, key, pv));
        else if (client is IPolicyConfigVista policyConfigVista)
            Marshal.ThrowExceptionForHR(policyConfigVista.GetPropertyValue(pszDeviceName, bFxStore, key, pv));
        else if (client is IPolicyConfig10 policyConfig10)
            Marshal.ThrowExceptionForHR(policyConfig10.GetPropertyValue(pszDeviceName, bFxStore, key, pv));
        else
            throw ExNone();
    }

    public void SetPropertyValue(string pszDeviceName, bool bFxStore, nint key, nint pv)
    {
        if (client is IPolicyConfig policyConfig)
            Marshal.ThrowExceptionForHR(policyConfig.SetPropertyValue(pszDeviceName, bFxStore, key, pv));
        else if (client is IPolicyConfigVista policyConfigVista)
            Marshal.ThrowExceptionForHR(policyConfigVista.SetPropertyValue(pszDeviceName, bFxStore, key, pv));
        else if (client is IPolicyConfig10 policyConfig10)
            Marshal.ThrowExceptionForHR(policyConfig10.SetPropertyValue(pszDeviceName, bFxStore, key, pv));
        else
            throw ExNone();
    }

#endif

    public void SetDefaultConsoleEndpoint(string pszDeviceName) => SetDefaultEndpoint(pszDeviceName, Role.Console);
    public void SetDefaultMultimediaEndpoint(string pszDeviceName) => SetDefaultEndpoint(pszDeviceName, Role.Multimedia);
    public void SetDefaultCommunicationsEndpoint(string pszDeviceName) => SetDefaultEndpoint(pszDeviceName, Role.Communications);

    public void SetDefaultEndpoint(string pszDeviceName, Role role)
    {
        if (client is IPolicyConfig policyConfig)
            Marshal.ThrowExceptionForHR(policyConfig.SetDefaultEndpoint(pszDeviceName, role));
        else if (client is IPolicyConfigVista policyConfigVista)
            Marshal.ThrowExceptionForHR(policyConfigVista.SetDefaultEndpoint(pszDeviceName, role));
        else if (client is IPolicyConfig10 policyConfig10)
            Marshal.ThrowExceptionForHR(policyConfig10.SetDefaultEndpoint(pszDeviceName, role));
        else
            throw ExNone();
    }

#if false

    public void SetEndpointVisibility(string pszDeviceName, bool bVisible)
    {
        if (client is IPolicyConfig policyConfig)
            Marshal.ThrowExceptionForHR(policyConfig.SetEndpointVisibility(pszDeviceName, bVisible));
        else if (client is IPolicyConfigVista policyConfigVista)
            Marshal.ThrowExceptionForHR(policyConfigVista.SetEndpointVisibility(pszDeviceName, bVisible));
        else if (client is IPolicyConfig10 policyConfig10)
            Marshal.ThrowExceptionForHR(policyConfig10.SetEndpointVisibility(pszDeviceName, bVisible));
        else
            throw ExNone();
    }
#endif

    private static InvalidOperationException ExNone() => new("No interfaces usable");
}