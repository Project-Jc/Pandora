using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Pandora.Misc
{
    public class EntryPoint
    {
        public static string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string appName = "";

        public static void Inject()
        {
            var _thread = new Thread(() => {
                var newDomain = AppDomain.CreateDomain("appName_InProcess_");
                newDomain.ExecuteAssembly($@"{ appPath }\{ appName }");
                AppDomain.Unload(newDomain);
            });

            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
        }
    }
}
