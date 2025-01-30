using System;
using System.Runtime.InteropServices;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;

namespace SandFoxGUI
{
    public class NativeInjector
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        private const int PROCESS_CREATE_THREAD = 0x0002;
        private const int PROCESS_QUERY_INFORMATION = 0x0400;
        private const int PROCESS_VM_OPERATION = 0x0008;
        private const int PROCESS_VM_WRITE = 0x0020;
        private const int PROCESS_VM_READ = 0x0010;
        private const uint MEM_RESERVE = 0x2000;
        private const uint MEM_COMMIT = 0x1000;
        private const uint PAGE_READWRITE = 0x04;
        private const uint PAGE_EXECUTE_READWRITE = 0x40;

        public static void InjectDLL(int processId, string dllPath)
        {
            if (!File.Exists(dllPath))
                throw new FileNotFoundException("DLL file not found", dllPath);

            // Get handle to the target process
            IntPtr processHandle = OpenProcess(
                PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ,
                false, processId);

            if (processHandle == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastPInvokeError(), "Failed to open target process.");

            try
            {
                // Allocate memory in the target process for the DLL path
                byte[] dllBytes = Encoding.ASCII.GetBytes(dllPath);
                uint bufferSize = (uint)((dllBytes.Length + 1) * Marshal.SizeOf(typeof(char)));

                IntPtr remoteBuffer = VirtualAllocEx(processHandle, IntPtr.Zero, bufferSize,
                    MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);

                if (remoteBuffer == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastPInvokeError(), "Failed to allocate memory in target process.");

                // Write the DLL path to the allocated memory
                UIntPtr bytesWritten;
                bool writeResult = WriteProcessMemory(processHandle, remoteBuffer, dllBytes,
                    bufferSize, out bytesWritten);

                if (!writeResult)
                    throw new Win32Exception(Marshal.GetLastPInvokeError(), "Failed to write to target process memory.");

                // Get the address of LoadLibraryA in kernel32.dll
                IntPtr kernel32Handle = GetModuleHandle("kernel32.dll");
                IntPtr loadLibraryAddr = GetProcAddress(kernel32Handle, "LoadLibraryA");

                if (loadLibraryAddr == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastPInvokeError(), "Failed to get LoadLibraryA address.");

                // Create a remote thread that calls LoadLibraryA with the DLL path as argument
                IntPtr remoteThread = CreateRemoteThread(processHandle, IntPtr.Zero, 0,
                    loadLibraryAddr, remoteBuffer, 0, IntPtr.Zero);

                if (remoteThread == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastPInvokeError(), "Failed to create remote thread.");
            }
            finally
            {
                if (processHandle != IntPtr.Zero)
                    CloseHandle(processHandle);
            }
        }

        public static Process GetProcessByName(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            return processes.FirstOrDefault()
                ?? throw new InvalidOperationException($"Process '{processName}' not found.");
        }
    }
}