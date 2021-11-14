using System.IO;
using System.Windows.Forms;
using NReco.VideoConverter;
using MediaToolkit;
using MediaToolkit.Options;
using System;
using MediaToolkit.Model;

namespace VideoEditor
{
    public class VideoProcessing
    {

        public static string FileNameWithoutExtension(string fileName)
        {
            return fileName.Split('.')[0];
        }

        public static void ConvertMp4ToMp3(string inputFile, string outputFile)
        {
            if (File.Exists(outputFile))
            {
                MessageBox.Show(outputFile + " already exists!");
                return;
            }
            var ConvertVideo = new FFMpegConverter();
            ConvertVideo.ConvertMedia(@inputFile, @outputFile, "mp3");
        }
        public static void ConcatVideo(string[] inputFile, string outputFile)
        {
            if (File.Exists(outputFile))
            {
                MessageBox.Show(outputFile + " already exists!");
                return;
            }
            var ConvertVideo = new FFMpegConverter();
            ConcatSettings cs = new ConcatSettings();

            ConvertVideo.ConcatMedia(inputFile, outputFile, "mp4", cs);
        }
        public static void CutVideo(string input, string output, TimeSpan start, TimeSpan duration)
        {
            var inputFile = new MediaFile { Filename = @input };
            var outputFile = new MediaFile { Filename = @output };

            using (var engine = new Engine())
            {
                var options = new ConversionOptions();
                options.CutMedia(start, duration);
                engine.Convert(inputFile, outputFile, options);
            }
        }
    }
}
