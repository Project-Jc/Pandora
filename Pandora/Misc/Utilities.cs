using System.Runtime.InteropServices;

namespace Pandora.Other
{
    public class Utilities
    {
        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();
    }
}
