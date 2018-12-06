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


        #region Read Memory

        public abstract T Read<T>(IntPtr ptr) where T : struct;

        public abstract T[] ReadArray<T>(IntPtr ptr, int size) where T : struct;


        public abstract IntPtr ReadIntPtr(IntPtr ptr);

        public abstract Byte ReadByte(IntPtr ptr);

        public abstract Byte[] ReadBytes(IntPtr ptr, int size);

        public abstract Boolean ReadBool(IntPtr ptr);

        public abstract Int16 ReadShort(IntPtr ptr);

        public abstract UInt16 ReadUShort(IntPtr ptr);

        public abstract Int32 ReadInt(IntPtr ptr);

        public abstract UInt32 ReadUInt(IntPtr ptr);

        public abstract Int64 ReadLong(IntPtr ptr);

        public abstract UInt64 ReadULong(IntPtr ptr);

        public abstract Single ReadFloat(IntPtr ptr);

        public abstract Double ReadDouble(IntPtr ptr);

        public abstract String ReadString(IntPtr ptr, System.Text.Encoding encoding, int length);

        #endregion

        #region Write Memory

        public abstract bool Write<T>(IntPtr ptr, T value) where T : struct;

        public abstract bool WriteProtected<T>(IntPtr ptr, T value) where T : struct;

        public abstract bool WriteArray<T>(IntPtr ptr, T[] value) where T : struct;


        public abstract bool Write(IntPtr ptr, byte[] values);

        public abstract bool Write(IntPtr ptr, byte value);

        public abstract bool Write(IntPtr ptr, short value);

        public abstract bool Write(IntPtr ptr, ushort value);

        public abstract bool Write(IntPtr ptr, int value);

        public abstract bool Write(IntPtr ptr, uint value);

        public abstract bool Write(IntPtr ptr, long value);

        public abstract bool Write(IntPtr ptr, ulong value);

        public abstract bool Write(IntPtr ptr, float value);

        public abstract bool Write(IntPtr ptr, double value);

        public void Dispose()
        {
            ((IDisposable)Process).Dispose();
        }

        #endregion

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int dwProcessId);
    }
}
