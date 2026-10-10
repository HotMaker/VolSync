using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace VolSync
{
    class Program
    {
        [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        class MMDeviceEnumeratorComObject { }

        enum EDataFlow { eRender = 0, eCapture = 1, eAll = 2 }
        enum ERole { eConsole = 0, eMultimedia = 1, eCommunications = 2 }

        [ComImport, Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IMMDeviceEnumerator
        {
            int EnumAudioEndpoints(EDataFlow dataFlow, int dwStateMask, out IntPtr ppDevices);
            int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppEndpoint);
            int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string pwstrId, out IMMDevice ppDevice);
            int RegisterEndpointNotificationCallback(IntPtr pClient);
            int UnregisterEndpointNotificationCallback(IntPtr pClient);
        }

        [ComImport, Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IMMDevice
        {
            int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
            int OpenPropertyStore(int stgmAccess, out IntPtr ppProperties);
            int GetId([MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);
            int GetState(out int pdwState);
        }

        [ComImport, Guid("5CDF2C82-841E-4546-9722-0CF74078229A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IAudioEndpointVolume
        {
            int RegisterControlChangeNotify(IntPtr pNotify);
            int UnregisterControlChangeNotify(IntPtr pNotify);
            int GetChannelCount(out int pnChannelCount);
            int SetMasterVolumeLevel(float fLevelDB, ref Guid pguidEventContext);
            int SetMasterVolumeLevelScalar(float fLevel, ref Guid pguidEventContext);
            int GetMasterVolumeLevel(out float pfLevelDB);
            int GetMasterVolumeLevelScalar(out float pfLevel);
            int SetChannelVolumeLevel(int nChannel, float fLevelDB, ref Guid pguidEventContext);
            int SetChannelVolumeLevelScalar(int nChannel, float fLevel, ref Guid pguidEventContext);
            int GetChannelVolumeLevel(int nChannel, out float pfLevelDB);
            int GetChannelVolumeLevelScalar(int nChannel, out float pfLevel);
            int SetMute(bool bMute, ref Guid pguidEventContext);
            int GetMute(out bool pbMute);
            int GetVolumeStepInfo(out int pnStep, out int pnStepCount);
            int VolumeStepUp(ref Guid pguidEventContext);
            int VolumeStepDown(ref Guid pguidEventContext);
            int QueryHardwareSupport(out int pdwHardwareSupportMask);
            int GetVolumeRange(out float pflVolumeMindB, out float pflVolumeMaxdB, out float pflVolumeIncrementdB);
        }

        // Flattened: base IAudioSessionManager methods declared first
        [ComImport, Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IAudioSessionManager2
        {
            int GetAudioSessionControl(IntPtr AudioSessionGuid, int StreamFlags, out IntPtr SessionControl);
            int GetSimpleAudioVolume(IntPtr AudioSessionGuid, int StreamFlags, out ISimpleAudioVolume AudioVolume);
            int GetSessionEnumerator(out IAudioSessionEnumerator SessionEnum);
            int RegisterSessionNotification(IntPtr SessionNotification);
            int UnregisterSessionNotification(IntPtr SessionNotification);
            int RegisterDuckNotification([MarshalAs(UnmanagedType.LPWStr)] string sessionID, IntPtr duckNotification);
            int UnregisterDuckNotification(IntPtr duckNotification);
        }

        [ComImport, Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IAudioSessionEnumerator
        {
            int GetCount(out int SessionCount);
            int GetSession(int SessionCount, out IAudioSessionControl Session);
        }

        [ComImport, Guid("F4B1A599-7266-4319-A8CA-E70ACB11E8CD"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IAudioSessionControl
        {
            int GetState(out int pRetVal);
            int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
            int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string Value, ref Guid EventContext);
            int GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
            int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string Value, ref Guid EventContext);
            int GetGroupingParam(out Guid pRetVal);
            int SetGroupingParam(ref Guid Override, ref Guid EventContext);
            int RegisterAudioSessionNotification(IntPtr NewNotifications);
            int UnregisterAudioSessionNotification(IntPtr NewNotifications);
        }

        // Flattened: base IAudioSessionControl methods declared first
        [ComImport, Guid("bfb7ff88-7239-4fc9-8fa2-07c950be9c6d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IAudioSessionControl2
        {
            int GetState(out int pRetVal);
            int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
            int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string Value, ref Guid EventContext);
            int GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
            int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string Value, ref Guid EventContext);
            int GetGroupingParam(out Guid pRetVal);
            int SetGroupingParam(ref Guid Override, ref Guid EventContext);
            int RegisterAudioSessionNotification(IntPtr NewNotifications);
            int UnregisterAudioSessionNotification(IntPtr NewNotifications);
            int GetSessionIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
            int GetSessionInstanceIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
            int GetProcessId(out int pRetVal);
            int IsSystemSoundsSession();
            int SetDuckingPreference(bool optOut);
        }

        [ComImport, Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface ISimpleAudioVolume
        {
            int SetMasterVolume(float fLevel, ref Guid EventContext);
            int GetMasterVolume(out float pfLevel);
            int SetMute(bool bMute, ref Guid EventContext);
            int GetMute(out bool pbMute);
        }

        static Guid IID_EndpointVolume = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");
        static Guid IID_SessionManager2 = new Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F");

        static string LOG = null;
        static void Log(string s)
        {
            if (LOG == null) return;
            try { File.AppendAllText(LOG, DateTime.Now.ToString("HH:mm:ss.fff") + " " + s + Environment.NewLine); } catch { }
        }

        static IMMDevice GetDefaultRender()
        {
            var en = (IMMDeviceEnumerator)(new MMDeviceEnumeratorComObject());
            IMMDevice dev;
            en.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eConsole, out dev);
            return dev;
        }

        static float ReadMaster(IMMDevice dev)
        {
            object o; var iid = IID_EndpointVolume;
            dev.Activate(ref iid, 1, IntPtr.Zero, out o);
            var vol = (IAudioEndpointVolume)o;
            float v; vol.GetMasterVolumeLevelScalar(out v);
            return v;
        }

        static void Main(string[] args)
        {
            int interval = 120;
            for (int i = 0; i < args.Length; i++)
                if (args[i] == "-log" && i + 1 < args.Length) LOG = args[i + 1];
            Log("=== VolSync start ===");
            Console.WriteLine("VolSync running...");

            var seen = new Dictionary<string, float>();
            float last = -1f;
            while (true)
            {
                try
                {
                    var dev = GetDefaultRender();
                    float master = ReadMaster(dev);
                    bool changed = Math.Abs(master - last) > 0.0005f;

                    object o; var iid = IID_SessionManager2;
                    dev.Activate(ref iid, 23, IntPtr.Zero, out o);
                    var mgr = (IAudioSessionManager2)o;
                    IAudioSessionEnumerator en; mgr.GetSessionEnumerator(out en);
                    int n; en.GetCount(out n);
                    var g = Guid.Empty;
                    int applied = 0;
                    for (int i = 0; i < n; i++)
                    {
                        try
                        {
                            IAudioSessionControl c; en.GetSession(i, out c);
                            var c2 = c as IAudioSessionControl2;
                            string key = null;
                            if (c2 != null) { try { c2.GetSessionInstanceIdentifier(out key); } catch { } }
                            var sav = c as ISimpleAudioVolume;
                            if (sav == null) continue;
                            bool isNew = (key != null && !seen.ContainsKey(key));
                            if (changed || isNew)
                            {
                                sav.SetMasterVolume(master, ref g);
                                applied++;
                            }
                            if (key != null) seen[key] = master;
                        }
                        catch { }
                    }
                    if (changed) Log("master=" + master.ToString("0.000") + " sessions=" + n + " applied=" + applied);
                    last = master;
                }
                catch (Exception e) { Log("ERR " + e.Message); }
                Thread.Sleep(interval);
            }
        }
    }
}
