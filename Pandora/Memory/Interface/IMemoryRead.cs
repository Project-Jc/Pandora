using System;

namespace Pandora
{
    public interface IMemoryRead
    {
        T Read<T>(IntPtr ptr) where T : struct;

        T[] ReadArray<T>(IntPtr ptr, int size) where T : struct;


        IntPtr ReadIntPtr(IntPtr ptr);

        Byte ReadByte(IntPtr ptr);

        Byte[] ReadBytes(IntPtr ptr, int size);

        Boolean ReadBool(IntPtr ptr);

        Int16 ReadShort(IntPtr ptr);

        UInt16 ReadUShort(IntPtr ptr);

        Int32 ReadInt(IntPtr ptr);

        UInt32 ReadUInt(IntPtr ptr);

        Int64 ReadLong(IntPtr ptr);

        UInt64 ReadULong(IntPtr ptr);

        Single ReadFloat(IntPtr ptr);

        Double ReadDouble(IntPtr ptr);

        String ReadString(IntPtr ptr, System.Text.Encoding encoding, int length);
    }
}
