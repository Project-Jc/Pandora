using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Pandora.ProcessCreation
{
    public sealed class ProcessCreator
    {
        public static bool Create(string pathToProcess, ProcessCreationFlags processCreationFlags, out Process process, out PROCESS_INFORMATION processInformation)
        {
            process = null;
            processInformation = default;

            if (!System.IO.File.Exists(pathToProcess))
                return false;

            STARTUPINFO startUpInfo = new STARTUPINFO();
            SECURITY_ATTRIBUTES threadAttributes = new SECURITY_ATTRIBUTES();
            SECURITY_ATTRIBUTES processAttributes = new SECURITY_ATTRIBUTES();

            var processCreated = 
                CreateProcess(pathToProcess, null, ref processAttributes, ref threadAttributes, false, processCreationFlags, IntPtr.Zero, null, ref startUpInfo, out processInformation);

            if (processCreated) {
                process = Process.GetProcessById(processInformation.dwProcessId);
            }
      
            return processCreated;
        }

        public static bool Create(string pathToProcess, out Process process, out PROCESS_INFORMATION processInformation) =>
            Create(pathToProcess, ProcessCreationFlags.CREATE_DEFAULT_ERROR_MODE, out process, out processInformation);

        public static bool CreateSuspended(string pathToProcess, out Process process, out PROCESS_INFORMATION processInformation) =>
            Create(pathToProcess, ProcessCreationFlags.CREATE_SUSPENDED, out process, out processInformation);


        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool CreateProcess(
            string lpApplicationName, string lpCommandLine, 
            ref SECURITY_ATTRIBUTES lpProcessAttributes, ref SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandles, 
            ProcessCreationFlags dwCreationFlags, 
            IntPtr lpEnvironment, 
            string lpCurrentDirectory, 
            [In] ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint ResumeThread(IntPtr hThread);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STARTUPINFO
    {
        public uint cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public uint dwX;
        public uint dwY;
        public uint dwXSize;
        public uint dwYSize;
        public uint dwXCountChars;
        public uint dwYCountChars;
        public uint dwFillAttribute;
        public uint dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public int dwProcessId;
        public int dwThreadId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SECURITY_ATTRIBUTES
    {
        public int nLength;
        public IntPtr lpSecurityDescriptor;
        public int bInheritHandle;
    }

    [Flags]
    public enum ProcessCreationFlags : uint
    {
        ZERO_FLAG = 0x00000000,
        CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
        CREATE_DEFAULT_ERROR_MODE = 0x04000000,
        CREATE_NEW_CONSOLE = 0x00000010,
        CREATE_NEW_PROCESS_GROUP = 0x00000200,
        CREATE_NO_WINDOW = 0x08000000,
        CREATE_PROTECTED_PROCESS = 0x00040000,
        CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
        CREATE_SECURE_PROCESS = 0x00400000,
        CREATE_SEPARATE_WOW_VDM = 0x00001000,
        CREATE_SHARED_WOW_VDM = 0x00001000,
        CREATE_SUSPENDED = 0x00000004,
        CREATE_UNICODE_ENVIRONMENT = 0x00000400,
        DEBUG_ONLY_THIS_PROCESS = 0x00000002,
        DEBUG_PROCESS = 0x00000001,
        DETACHED_PROCESS = 0x00000008,
        EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
        INHERIT_PARENT_AFFINITY = 0x00010000
    }
}
