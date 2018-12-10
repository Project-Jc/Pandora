using System;
using System.Runtime.InteropServices;


namespace Pandora
{
    public sealed class MemoryProtectionOperation : IDisposable, IMemoryOperation
    {
        private IntPtr HProcess;
        private IntPtr Address;
        private int Size;
        private MemoryProtectionType FlNewProtect;
        private MemoryProtectionType FlOldProtect;
        private bool OutOfProcess;


        /// <summary>
        /// Out of process memory protection operation.
        /// </summary>
        /// <param name="hProcess"></param>
        /// <param name="address"></param>
        /// <param name="size"></param>
        /// <param name="flNewProtect"></param>
        public MemoryProtectionOperation(IntPtr hProcess, IntPtr address, int size, MemoryProtectionType flNewProtect = MemoryProtectionType.PAGE_EXECUTE_READWRITE)
        {
            HProcess = hProcess;
            Address = address;
            Size = size;
            FlNewProtect = flNewProtect;
            OutOfProcess = true;
        }

        /// <summary>
        /// In process memory protection operation.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="size"></param>
        /// <param name="flNewProtect"></param>
        public MemoryProtectionOperation(IntPtr address, int size, MemoryProtectionType flNewProtect = MemoryProtectionType.PAGE_EXECUTE_READWRITE) :
            this(IntPtr.Zero, address, size, flNewProtect)
        {
            OutOfProcess = false;
        }


        public bool Apply()
        {
            if (!IsApplied) {
                IsApplied = OutOfProcess ?
                    VirtualProtectEx(HProcess, Address, Size, FlNewProtect, out FlOldProtect) :
                    VirtualProtect(Address, Size, FlNewProtect, out FlOldProtect);
            } return IsApplied;
        }

        public bool Remove()
        {
            if (IsApplied) {
                IsApplied = OutOfProcess ?
                     VirtualProtectEx(HProcess, Address, Size, FlOldProtect, out _) :
                     VirtualProtect(Address, Size, FlOldProtect, out _);
            } return IsApplied;
        }

        public bool IsApplied { get; private set; }

        public void Dispose() => Remove();


        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, MemoryProtectionType flNewProtect, out MemoryProtectionType lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualProtect(IntPtr lpAddress, int dwSize, MemoryProtectionType flNewProtect, out MemoryProtectionType lpflOldProtect);
    }
}
