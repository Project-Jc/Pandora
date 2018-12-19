using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Pandora
{
    public abstract class MemoryEditor : IDisposable
    {
        public Process Process { get; private set; }

        public IntPtr ProcessHandle { get; private set; }


        public MemoryEditor(Process process) => 
            AttachToProcess(process);

        public MemoryEditor()
        {
        }


        public static Process[] GetProcesses(string processName) => Process.GetProcessesByName(processName);


        public void AttachToProcess(Process process)
        {
            Process = process;
            ProcessHandle = OpenProcess(0x001F0FFF, false, process.Id);
        }

        public bool TryAttachToProcess(Process process)
        {
            if (CanAttachToProcess(process))
                AttachToProcess(process);

            return IsAttachedToProcess;
        }
        public bool TryAttachToProcess(int processId) => TryAttachToProcess(Process.GetProcessById(processId));
        public bool TryAttachToProcess(string processName) => TryAttachToProcess(Process.GetProcessesByName(processName).FirstOrDefault());

        public static bool CanAttachToProcess(Process process) => process?.Handle != IntPtr.Zero;
        public static bool CanAttachToProcess(int processId) => CanAttachToProcess(Process.GetProcessById(processId));
        public static bool CanAttachToProcess(string processName) => CanAttachToProcess(GetProcesses(processName).FirstOrDefault());

        public bool IsAttachedToProcess => Process != null;
        public void DetachFromProcess() => Process = null;


        public IntPtr MainModule => Process.MainModule.BaseAddress;

        public string FilePath => Process.MainModule.FileName.Substring(0, (Process.MainModule.FileName.LastIndexOf("\\") + 1));
        public string FileName => Path.GetFileName(Process.MainModule.FileName);
        public string FileVersion => Process.MainModule.FileVersionInfo.FileVersion;
        public string StartTime => Process.StartTime.ToString();

        public void Dispose()
        {
            ((IDisposable)Process).Dispose();
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int dwProcessId);
    }
}
