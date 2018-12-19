using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Pandora
{
    public class OutOfProcessMemoryEditor : MemoryEditor, IMemoryRead, IMemoryWrite
    {
        public OutOfProcessMemoryEditor()
            : base()
        {

        }


        public bool HasModule(string moduleName) => GetModule(moduleName) != null;

        public ProcessModule GetModule(string moduleName, StringComparison stringComparison = StringComparison.CurrentCulture) =>
            Process.Modules.Cast<ProcessModule>().FirstOrDefault(f => f.ModuleName.Equals(moduleName, stringComparison));


        #region Read Memory

        public T Read<T>(IntPtr address) where T : struct
        {
            //T buffer = default(T); // Doesn't work... Must expect an array of some kind.
            T[] buffer = new T[] { default(T) };
            ReadProcessMemory(Process.Handle, address, buffer, Marshal.SizeOf<T>(), out var bytesread);
            return buffer[0];
        }

        public T[] ReadArray<T>(IntPtr address, int size) where T : struct
        {
            T[] buffer = new T[size];
            ReadProcessMemory(Process.Handle, address, buffer, size, out var bytesread);
            return buffer;
        }

        public T ReadRelative<T>(int offset)
        {
            T[] buffer = new T[] { default(T) };
            ReadProcessMemory(Process.Handle, Process.MainModule.BaseAddress + offset, buffer, Marshal.SizeOf<T>(), out var bytesread);
            return buffer[0];
        }

        public T[] ReadArrayRelative<T>(int offset, int size)
        {
            T[] buffer = new T[size];
            ReadProcessMemory(Process.Handle, Process.MainModule.BaseAddress + offset, buffer, size, out var bytesread);
            return buffer;
        }


        public IntPtr ReadIntPtr(IntPtr address) =>
            new IntPtr(ReadInt(address));
               
        public Byte ReadByte(IntPtr address) =>
            ReadProcessMemory(address, 1)[0];
               
        public Byte[] ReadBytes(IntPtr address, int size) =>
            ReadProcessMemory(address, size);
               
        public Boolean ReadBool(IntPtr address) =>
            Convert.ToBoolean(ReadByte(address));
               
        public Int16 ReadShort(IntPtr address) =>
            BitConverter.ToInt16(ReadProcessMemory(address, sizeof(short)), 0);
               
        public UInt16 ReadUShort(IntPtr address) =>
            BitConverter.ToUInt16(ReadProcessMemory(address, sizeof(short)), 0);
               
        public Int32 ReadInt(IntPtr address) =>
            BitConverter.ToInt32(ReadProcessMemory(address, sizeof(int)), 0);
               
        public UInt32 ReadUInt(IntPtr address) =>
            BitConverter.ToUInt32(ReadProcessMemory(address, sizeof(int)), 0);
               
        public Int64 ReadLong(IntPtr address) =>
            BitConverter.ToInt64(ReadProcessMemory(address, sizeof(long)), 0);
               
        public UInt64 ReadULong(IntPtr address) =>
            BitConverter.ToUInt64(ReadProcessMemory(address, sizeof(long)), 0);
               
        public Single ReadFloat(IntPtr address) =>
            BitConverter.ToSingle(ReadProcessMemory(address, sizeof(float)), 0);
               
        public Double ReadDouble(IntPtr address) =>
            BitConverter.ToDouble(ReadProcessMemory(address, sizeof(double)), 0);
               
        public String ReadString(IntPtr address, Encoding encoding, int length) =>
            encoding.GetString(ReadProcessMemory(address, length));

        #endregion

        #region Write Memory

        public bool Write<T>(IntPtr baseaddress, T value) where T : struct
        { 
            //var buffer = new T[Marshal.SizeOf<T>()];
            //buffer[0] = value;
            return WriteProcessMemory(Process.Handle, baseaddress, value, Marshal.SizeOf<T>(), out var numberOfBytesWritten);
        }

        public bool WriteProtected<T>(IntPtr address, T value) where T : struct
        {
            using (var memoryOperation = new MemoryProtectionOperation(address, Marshal.SizeOf<T>())) {
                if (memoryOperation.Apply()) {
                    return Write(address, value);
                }
            } return false;
        }

        public bool WriteArray<T>(IntPtr baseaddress, T[] value) where T : struct => 
            WriteProcessMemory(Process.Handle, baseaddress, value, value.Length, out var numberOfBytesWritten);


        public bool Write(IntPtr address, byte[] values) =>
            WriteProcessMemory(Process.Handle, address, values, values.Length, out var numberOfBytesWritten);

        public bool Write(IntPtr address, byte value) =>
            Write(address, new byte[] { value });

        public bool Write(IntPtr address, short value) =>
            Write(address, BitConverter.GetBytes(value));

        public bool Write(IntPtr address, ushort value) =>
            Write(address, BitConverter.GetBytes(value));

        public bool Write(IntPtr address, int value) =>
            Write(address, BitConverter.GetBytes(value));

        public bool Write(IntPtr address, uint value) =>
            Write(address, BitConverter.GetBytes(value));

        public bool Write(IntPtr address, long value) =>
            Write(address, BitConverter.GetBytes(value));

        public bool Write(IntPtr address, ulong value) =>
            Write(address, BitConverter.GetBytes(value));

        public bool Write(IntPtr address, float value) =>
            Write(address, BitConverter.GetBytes(value));

        public bool Write(IntPtr address, double value) =>
            Write(address, BitConverter.GetBytes(value));

        #endregion 


        private byte[] ReadProcessMemory(IntPtr address, int size)
        {
            var buffer = new byte[size];
            ReadProcessMemory(Process.Handle, address, buffer, size, out var numberOfBytesRead);
            return buffer;
        }


        [DllImport("kernel32.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseaddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseaddress, [Out, MarshalAs(UnmanagedType.AsAny)] object lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseaddress, byte[] lpBuffer, Int32 nSize, out IntPtr lpNumberOfBytesWritten);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpaddress, [MarshalAs(UnmanagedType.AsAny)] object lpBuffer, int dwSize, out IntPtr lpNumberOfBytesWritten);
    }
}
