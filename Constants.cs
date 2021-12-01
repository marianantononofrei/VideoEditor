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
        public static readonly string[] SUPPORTED_VIDEO_FORMATS = { ".mp4", ".avi", ".wmv", ".mkv", ".flv", ".mov" };
        public static readonly string[] SUPPORTED_AUDIO_FORMATS = { ".mp3", ".wav" };
        public static readonly string[] SUPPORTED_IMAGE_FORMATS = { ".jpg", ".jpeg", ".png", ".bmp", ".tif" };

    }
}
