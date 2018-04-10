using System.IO;
using System.Reflection;

namespace HotTips
{
    class Utils
    {

        public static string GetLocalExtensionDir()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

    }
}
