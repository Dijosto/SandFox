using System;
using System.Runtime.InteropServices;

namespace SandFoxGUI
{
    public class NativeBootstrapper
    {
        [DllImport("mscoree.dll")]
        private static extern int CLRCreateInstance(ref Guid clsid, ref Guid riid, out ICLRMetaHost ppInterface);

        [ComImport]
        [Guid("D332DB9E-B9B3-4125-8207-A14884F53216")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ICLRMetaHost
        {
            void GetRuntime([MarshalAs(UnmanagedType.LPWStr)] string pwzVersion, ref Guid riid, out ICLRRuntimeInfo ppRuntime);
            // Other methods omitted for brevity
        }

        [ComImport]
        [Guid("BD39D1D2-BA2F-486A-89B0-B4B0CB466891")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ICLRRuntimeInfo
        {
            void GetRuntimeHost(out ICLRRuntimeHost ppRuntime);
            // Other methods omitted for brevity
        }

        [ComImport]
        [Guid("90F1A06E-7712-4762-86B5-7A5EBA6BDB02")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ICLRRuntimeHost
        {
            void Start();
            void ExecuteInDefaultAppDomain([MarshalAs(UnmanagedType.LPWStr)] string pwzAssemblyPath,
                [MarshalAs(UnmanagedType.LPWStr)] string pwzTypeName,
                [MarshalAs(UnmanagedType.LPWStr)] string pwzMethodName,
                [MarshalAs(UnmanagedType.LPWStr)] string pwzArgument,
                out uint pReturnValue);
            // Other methods omitted for brevity
        }

        public static void Bootstrap()
        {
            try
            {
                // Get the CLR host
                Guid CLSID_CLRMetaHost = new Guid("9280188D-0E8E-4867-B30C-7FA83884E8DE");
                Guid IID_ICLRMetaHost = new Guid("D332DB9E-B9B3-4125-8207-A14884F53216");
                ICLRMetaHost metaHost;
                CLRCreateInstance(ref CLSID_CLRMetaHost, ref IID_ICLRMetaHost, out metaHost);

                // Get the runtime
                Guid IID_ICLRRuntimeInfo = new Guid("BD39D1D2-BA2F-486A-89B0-B4B0CB466891");
                ICLRRuntimeInfo runtimeInfo;
                metaHost.GetRuntime("v4.0.30319", ref IID_ICLRRuntimeInfo, out runtimeInfo);

                // Get the runtime host
                ICLRRuntimeHost runtimeHost;
                runtimeInfo.GetRuntimeHost(out runtimeHost);

                // Start the runtime
                runtimeHost.Start();

                // Execute our managed entry point
                uint returnValue;
                runtimeHost.ExecuteInDefaultAppDomain(
                    "ManagedPayload.dll",
                    "ManagedPayload.Program",
                    "PayloadMain",
                    "",
                    out returnValue);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bootstrap Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}