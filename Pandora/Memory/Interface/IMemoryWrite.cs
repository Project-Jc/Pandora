using System;

namespace Pandora
{
    public interface IMemoryWrite
    {
        bool Write<T>(IntPtr ptr, T value) where T : struct;

        bool WriteProtected<T>(IntPtr ptr, T value) where T : struct;

        bool WriteArray<T>(IntPtr ptr, T[] value) where T : struct;


        bool Write(IntPtr ptr, Byte[] values);

        bool Write(IntPtr ptr, Byte value);

        bool Write(IntPtr ptr, Int16 value);

        bool Write(IntPtr ptr, UInt16 value);

        bool Write(IntPtr ptr, Int32 value);

        bool Write(IntPtr ptr, UInt32 value);

        bool Write(IntPtr ptr, Int64 value);

        bool Write(IntPtr ptr, UInt64 value);

        bool Write(IntPtr ptr, Single value);

        bool Write(IntPtr ptr, Double value);
    }
}
