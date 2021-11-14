using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VideoEditor
{
    public static class Constants
    {
        public static string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;
    }
}
