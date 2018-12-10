using System.Runtime.InteropServices;

namespace Pandora.Misc
{
    public class Utilities
    {
        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();
    }
}
