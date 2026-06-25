using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using NAudio.CoreAudioApi;
using static System.Windows.Forms.DataFormats;
using static CoreAudioApi.Interfaces.PolicyConfigClient;

namespace CoreAudioApi.Interfaces;

internal sealed partial class PolicyConfigClient
{
    [ComImport, Guid("870af99c-171d-4f9e-af0d-e63df40c2bc9")]
    private class _PolicyConfigClient;

    private readonly _PolicyConfigClient client = new();

    private IPolicyConfig? policyConfig => client as IPolicyConfig;
    private IPolicyConfigVista? policyConfigVista => client as IPolicyConfigVista;
    private IPolicyConfig10? policyConfig10 => client as IPolicyConfig10;

    public WAVEFORMATEXTENSIBLE GetMixFormat(string pszDeviceName)
    {
        WAVEFORMATEXTENSIBLE format;
        if (client is IPolicyConfig policyConfig)
            Marshal.ThrowExceptionForHR(policyConfig.GetMixFormat(pszDeviceName, out format));
        else if (client is IPolicyConfigVista policyConfigVista)
            Marshal.ThrowExceptionForHR(policyConfigVista.GetMixFormat(pszDeviceName, out format));
        else if (client is IPolicyConfig10 policyConfig10)
            Marshal.ThrowExceptionForHR(policyConfig10.GetMixFormat(pszDeviceName, out format));
        else
            throw ExNone();
        return format;
    }

    public WAVEFORMATEXTENSIBLE GetDeviceFormat(string pszDeviceName, bool bDefault)
    {
        WAVEFORMATEXTENSIBLE format;
        if (client is IPolicyConfig policyConfig)
            Marshal.ThrowExceptionForHR(policyConfig.GetDeviceFormat(pszDeviceName, bDefault, out format));
        else if (client is IPolicyConfigVista policyConfigVista)
            Marshal.ThrowExceptionForHR(policyConfigVista.GetDeviceFormat(pszDeviceName, bDefault, out format));
        else if (client is IPolicyConfig10 policyConfig10)
            Marshal.ThrowExceptionForHR(policyConfig10.GetDeviceFormat(pszDeviceName, bDefault, out format));
        else
            throw ExNone();
        return format;
    }

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

    public DeviceShareMode GetShareMode(string pszDeviceName)
    {
        DeviceShareMode pMode;
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

    public void SetShareMode(string pszDeviceName, DeviceShareMode mode)
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

    public void SetDefaultEndpoint(string pszDeviceName, ERole role)
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