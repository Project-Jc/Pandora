using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Pandora.ProcessCreation
{ 
    public sealed class ProcessCreator
    {
        public readonly string PathToProcess;
        public readonly WindowState WindowState;
        public readonly StartUpInfo StartUpInfo;


        public ProcessCreator(string pathToProcess, WindowState windowState = WindowState.SW_SHOWNORMAL, StartUpInfo? startUpInfo = null)
        {
            PathToProcess = pathToProcess;
            WindowState = windowState;
            StartUpInfo = startUpInfo ?? new StartUpInfo() {
                dwFlags = 0x00000001,
                wShowWindow = (short)WindowState
            };
        }


        public async Task<CreateProcessData> CreateTaskAsync()
        {
            CreateProcessData createProcessData = new CreateProcessData();

            if (!System.IO.File.Exists(PathToProcess))
                return createProcessData;

            StartUpInfo startUpInfo = StartUpInfo;
            ProcessInformation processInformation;
            SecurityAttributes threadAttributes = new SecurityAttributes();
            SecurityAttributes processAttributes = new SecurityAttributes();

            var processCreated =
                CreateProcess(
                    PathToProcess,
                    null,
                    ref processAttributes, ref threadAttributes,
                    false,
                    ProcessCreationFlags.CREATE_DEFAULT_ERROR_MODE,
                    IntPtr.Zero,
                    null,
                    ref startUpInfo, out processInformation);

            if (processCreated) {

                createProcessData.Process = Process.GetProcessById(processInformation.dwProcessId);
                createProcessData.ProcessInformation = processInformation;

                return await Task.Run(() => {

                    // 5 second timeout...

                    for (
                        int cTick = Environment.TickCount, eTick = (cTick + 5_000);
                        cTick < eTick || !createProcessData.WindowLoaded;
                        cTick++) {

                        createProcessData.Process.Refresh();

                        if (!string.IsNullOrWhiteSpace(createProcessData.Process.MainWindowTitle)) {
                            createProcessData.WindowLoaded = true;
                        }
                    }

                    return createProcessData;
                });
            }

            return createProcessData;
        }
        

        public static async Task<bool> CreateTaskAsync(string pathToProcess)
        {
            if (!System.IO.File.Exists(pathToProcess))
                return false;

            StartUpInfo startUpInfo = new StartUpInfo() {
                dwFlags = 0x00000001,
                wShowWindow = 7
            };

            ProcessInformation processInformation;
            SecurityAttributes threadAttributes = new SecurityAttributes();
            SecurityAttributes processAttributes = new SecurityAttributes();

            var processCreated =
                CreateProcess(pathToProcess, null, ref processAttributes, ref threadAttributes, false, ProcessCreationFlags.CREATE_DEFAULT_ERROR_MODE, IntPtr.Zero, null, ref startUpInfo, out processInformation);

            if (processCreated) {

                Process process = Process.GetProcessById(processInformation.dwProcessId);

                return await Task.Run(() => {
                    // Sometimes, when creating a new process, the MainWindowHandle property is zero until the two while loops finish...
                    //
                    while (!process.WaitForInputIdle(100)) {
                    }
                    while (string.IsNullOrWhiteSpace(process.MainWindowTitle)) {
                    }
                    return true;
                });
            }

            return processCreated;
        }

        public static bool Create(string pathToProcess, ProcessCreationFlags processCreationFlags, out Process process, out ProcessInformation processInformation)
        {
            process = null;
            processInformation = default;

            if (!System.IO.File.Exists(pathToProcess))
                return false;

            StartUpInfo startUpInfo = new StartUpInfo();
            SecurityAttributes threadAttributes = new SecurityAttributes();
            SecurityAttributes processAttributes = new SecurityAttributes();

            var processCreated =
                CreateProcess(pathToProcess, null, ref processAttributes, ref threadAttributes, false, processCreationFlags, IntPtr.Zero, null, ref startUpInfo, out processInformation);

            if (processCreated)
                process = Process.GetProcessById(processInformation.dwProcessId);

            return processCreated;
        }

        public static bool CreateDefault(string pathToProcess, out Process process, out ProcessInformation processInformation) =>
            Create(pathToProcess, ProcessCreationFlags.CREATE_DEFAULT_ERROR_MODE, out process, out processInformation);

        public static bool CreateSuspended(string pathToProcess, out Process process, out ProcessInformation processInformation) =>
            Create(pathToProcess, ProcessCreationFlags.CREATE_SUSPENDED, out process, out processInformation);


        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool CreateProcess(
            string lpApplicationName, string lpCommandLine, 
            ref SecurityAttributes lpProcessAttributes, ref SecurityAttributes lpThreadAttributes,
            bool bInheritHandles, 
            ProcessCreationFlags dwCreationFlags, 
            IntPtr lpEnvironment, 
            string lpCurrentDirectory, 
            [In] ref StartUpInfo lpStartupInfo, out ProcessInformation lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint ResumeThread(IntPtr hThread);
    }

    public enum WindowState : short
    {
        SW_HIDE = 0,
        SW_SHOWNORMAL = 1,
        SW_SHOWMINIMIZED = 2,
        SW_MAXIMIZE = 3,
        SW_MINIMIZE = 6,
        SW_SHOWMINNOACTIVE = 7,
        SW_FORCEMINIMIZE = 11
    }

    public struct CreateProcessData
    {
        public bool WindowLoaded;
        public Process Process;
        public ProcessInformation ProcessInformation;

        //public CreateProcessData(bool result, Process process, ProcessInformation processInformation)
        //{
        //    Result = result;
        //    Process = process;
        //    ProcessInformation = processInformation;
        //}
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct StartUpInfo
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
    public struct ProcessInformation
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public int dwProcessId;
        public int dwThreadId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SecurityAttributes
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
