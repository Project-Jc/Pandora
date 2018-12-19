using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Pandora
{
    public unsafe class InProcessMemoryEditor : MemoryEditor, IMemoryRead, IMemoryWrite
    {
        public InProcessMemoryEditor() 
            : base(Process.GetCurrentProcess())
        {

        }


        public Detour CreateDetour(Delegate originalFunction, Delegate detourFunction) => 
            new Detour(originalFunction, detourFunction, this);

        public T FuncPtrToDelegate<T>(IntPtr address) where T : class => 
            Marshal.GetDelegateForFunctionPointer<T>(address);


        public T Read<T>(IntPtr ptr) where T : struct =>
            Marshal.PtrToStructure<T>(ptr);

        public T[] ReadArray<T>(IntPtr ptr, int size) where T : struct
        {
            var array = new T[size];
            for (int i = 0, vSize = Marshal.SizeOf<T>(); i < size; i++) {
                array[i] = Read<T>(ptr + (i * vSize));
            } return array;
        }

        public bool ReadBool(IntPtr ptr) => *(bool*)ptr;

        public byte ReadByte(IntPtr ptr) => *(byte*)ptr;

        public byte[] ReadBytes(IntPtr ptr, int size)
        {
            byte[] bytes = new byte[size];

            for (int i = 0; i < size; i++) 
                bytes[i] = ReadByte(ptr + i);
            
            return bytes;
        }

        public byte[] ReadBytes(byte* ptr, int size)
        {
            byte[] bytes = new byte[size];

            for (int i = 0; i < size; i++)
                bytes[i] = *(ptr + i);

            return bytes;
        }

        public double ReadDouble(IntPtr ptr) => *(double*)ptr;

        public float ReadFloat(IntPtr ptr) => *(float*)ptr;

        public int ReadInt(IntPtr ptr) => *(int*)ptr;

        public IntPtr ReadIntPtr(IntPtr ptr) => new IntPtr(ReadInt(ptr));

        public long ReadLong(IntPtr ptr) => *(long*)ptr;

        public short ReadShort(IntPtr ptr) => *(short*)ptr;

        public string ReadString(IntPtr ptr, Encoding encoding, int length) => encoding.GetString(ReadBytes(ptr, length));

        public uint ReadUInt(IntPtr ptr) => *(uint*)ptr;

        public ulong ReadULong(IntPtr ptr) => *(ulong*)ptr;

        public ushort ReadUShort(IntPtr ptr) => *(ushort*)ptr;


        public bool Write<T>(IntPtr ptr, T value) where T : struct
        {
            Marshal.StructureToPtr(value, ptr, false);
            return (Read<T>(ptr).Equals(value));
        }

        public bool WriteProtected<T>(IntPtr ptr, T value) where T : struct
        {
            using (var memoryOperation = new MemoryProtectionOperation(ptr, Marshal.SizeOf<T>())) {
                if (memoryOperation.Apply()) {
                    return Write(ptr, value);
                }
            } return false;
        }

        public bool WriteArray<T>(IntPtr ptr, T[] value) where T : struct
        {
            for (int i = 0, vSize = Marshal.SizeOf<T>(); i < value.Length; i++) {
                if (!Write(ptr + (i * vSize), value[i])) {
                    return false;
                }
            } return true;
        }


        public bool Write(IntPtr ptr, byte[] values)
        {
            for (int i = 0; i < values.Length; i++)
                if (!Write(ptr + i, values[i]))
                    return false;
            return true;
        }

        public bool Write(IntPtr ptr, byte value) => (*(byte*)ptr = value) == ReadByte(ptr);

        public bool Write(IntPtr ptr, short value) => (*(short*)ptr = value) == ReadShort(ptr);

        public bool Write(IntPtr ptr, ushort value) => (*(ushort*)ptr = value) == ReadUShort(ptr);

        public bool Write(IntPtr ptr, int value) => (*(int*)ptr = value) == ReadInt(ptr);

        public bool Write(IntPtr ptr, uint value) => (*(uint*)ptr = value) == ReadUInt(ptr);

        public bool Write(IntPtr ptr, long value) => (*(long*)ptr = value) == ReadLong(ptr);

        public bool Write(IntPtr ptr, ulong value) => (*(ulong*)ptr = value) == ReadULong(ptr);

        public bool Write(IntPtr ptr, float value) => (*(float*)ptr = value) == ReadFloat(ptr);

        public bool Write(IntPtr ptr, double value) => (*(double*)ptr = value) == ReadDouble(ptr);
    }
}
