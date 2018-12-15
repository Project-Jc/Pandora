using Pandora;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Pandora
{
    public enum OpCodes : byte
    {
        Push = 0x68,
        Return = 0xC3
    }

    public class Detour : IMemoryOperation
    {
        private Delegate OriginalFunction { get; }
        private IntPtr OriginalFunctionPtr { get; }

        private Delegate DetourFunction { get; }
        private IntPtr DetourFunctionPtr { get; }

        private byte[] OriginalInstructions;
        private List<byte> DetourInstructions;

        private InProcessMemoryEditor MemoryEditor;

        private MemoryProtectionOperation MemoryOperation;

        private const int instructionLength = 6;


        public Detour(Delegate originalFunction, Delegate detourFunction, InProcessMemoryEditor memoryEditor = null)
        {
            OriginalFunction = originalFunction;
            OriginalFunctionPtr = Marshal.GetFunctionPointerForDelegate(OriginalFunction);

            DetourFunction = detourFunction;
            DetourFunctionPtr = Marshal.GetFunctionPointerForDelegate(DetourFunction);

            DetourInstructions = new List<byte>() { (byte)OpCodes.Push, (byte)OpCodes.Return };
            DetourInstructions.InsertRange(1, BitConverter.GetBytes(DetourFunctionPtr.ToInt32()));

            MemoryEditor = memoryEditor ?? new InProcessMemoryEditor();
            MemoryOperation = new MemoryProtectionOperation(OriginalFunctionPtr, instructionLength);
        }


        public bool IsApplied { get; private set; }

        public bool Apply()
        {
            if (!IsApplied) {
                using (MemoryOperation) {
                    if (MemoryOperation.Apply()) {
                        OriginalInstructions = MemoryEditor.ReadBytes(OriginalFunctionPtr, instructionLength);
                        IsApplied = MemoryEditor.Write(OriginalFunctionPtr, DetourInstructions.ToArray());
                    }
                }
            }
            return IsApplied;
        }

        public bool Remove()
        {
            if (IsApplied) {
                using (MemoryOperation) {
                    if (MemoryOperation.Apply()) {
                        IsApplied = !MemoryEditor.Write(OriginalFunctionPtr, OriginalInstructions);
                    }
                }
            }
            return !IsApplied;
        }
    }
}

