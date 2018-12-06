using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Pandora.DllInjection
{
    public sealed class DllInjector
    {
        public static DllInjectResult Inject(int dwProcessId, string dll)
        {
            if (!System.IO.File.Exists(dll))
                return DllInjectResult.Failure | DllInjectResult.DllNotFound;

            try {

                // Open the process for modification.
                //
                IntPtr hProcess = OpenProcess(ProcessAccessFlags.All, false, dwProcessId);
                if (hProcess == IntPtr.Zero)
                    return DllInjectResult.Failure | DllInjectResult.OpeningProcess;

                // Get the address of LoadLibrary.
                //
                IntPtr loadLibraryAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
                if (loadLibraryAddress == IntPtr.Zero)
                    return DllInjectResult.Failure | DllInjectResult.FindingLoadLibrary;

                // Allocated a region of memory.
                //
                IntPtr allocatedMemory = VirtualAllocEx(hProcess, IntPtr.Zero, (IntPtr)dll.Length, AllocationType.Commit, MemoryProtection.ExecuteReadWrite);
                if (allocatedMemory == IntPtr.Zero)
                    return DllInjectResult.Failure | DllInjectResult.AllocatingMemory;

                // Write the name of our DLL to memory.
                //
                byte[] dllBytes = Encoding.ASCII.GetBytes(dll);
                bool dataWritten = WriteProcessMemory(hProcess, allocatedMemory, dllBytes, dllBytes.Length, out var lpNumberOfBytesWritten);
                if (!dataWritten)
                    return DllInjectResult.Failure | DllInjectResult.WritingMemory;

                // Have LoadLibrary load our DLL by creating a remote thread.
                //
                IntPtr remoteThread = CreateRemoteThread(hProcess, IntPtr.Zero, IntPtr.Zero, loadLibraryAddress, allocatedMemory, 0, IntPtr.Zero);
                if (remoteThread == IntPtr.Zero)
                    return DllInjectResult.Failure | DllInjectResult.CreatingRemoteThread;

                // Wait for the thread to exit.
                //
                WaitForSingleObject(remoteThread, WaitObject.INFINITE);

                // Free the allocated memory.
                //
                bool memoryReleased = VirtualFreeEx(hProcess, allocatedMemory, 0, AllocationType.Release);
                if (!memoryReleased)
                    return DllInjectResult.Failure | DllInjectResult.ReleasingMemory;

                // Close the handle.
                //
                CloseHandle(hProcess);
            }
            catch {
            }

            return DllInjectResult.Success;
        }

        public static bool TryInject(int dwProcessId, string dll) =>
            Inject(dwProcessId, dll).HasFlag(DllInjectResult.Success) ? true : false;

        public static bool TryInject(int dwProcessId, string dll, out DllInjectResult result) =>
            (result = Inject(dwProcessId, dll)).HasFlag(DllInjectResult.Success) ? true : false;


        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] bBuffer, Int32 nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

        [DllImport("kernel32.dll", EntryPoint = "VirtualFreeEx")]
        static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, AllocationType dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);
    }

    public struct WaitObject
    {
        public const UInt32 INFINITE = 0xFFFFFFFF;
        public const UInt32 WAIT_ABANDONED = 0x00000080;
        public const UInt32 WAIT_OBJECT_0 = 0x00000000;
        public const UInt32 WAIT_TIMEOUT = 0x00000102;
    }

    [Flags]
    public enum DllInjectResult
    {
        Success = 1,
        Failure = 2,

        ExeNotFound = 4,
        DllNotFound = 8,
        OpeningProcess = 16,
        FindingLoadLibrary = 32,
        AllocatingMemory = 64,
        WritingMemory = 128,
        CreatingRemoteThread = 256,
        CreatingProcess = 512,
        ReleasingMemory = 1024
    }

    [Flags]
    public enum AllocationType
    {
        Commit = 0x1000,
        Reserve = 0x2000,
        Decommit = 0x4000,
        Release = 0x8000,
        Reset = 0x80000,
        Physical = 0x400000,
        TopDown = 0x100000,
        WriteWatch = 0x200000,
        LargePages = 0x20000000
    }

    [Flags]
    public enum MemoryProtection
    {
        Execute = 0x10,
        ExecuteRead = 0x20,
        ExecuteReadWrite = 0x40,
        ExecuteWriteCopy = 0x80,
        NoAccess = 0x01,
        ReadOnly = 0x02,
        ReadWrite = 0x04,
        WriteCopy = 0x08,
        GuardModifierflag = 0x100,
        NoCacheModifierflag = 0x200,
        WriteCombineModifierflag = 0x400
    }

    [Flags]
    public enum ProcessAccessFlags : uint
    {
        All = 0x001F0FFF
    }
}
