using System.IO;
using System.Windows.Forms;
using NReco.VideoConverter;
using MediaToolkit;
using MediaToolkit.Options;
using System;
using MediaToolkit.Model;
using System.Drawing;
using NAudio.Wave;
using NAudio.WaveFormRenderer;
using WMPLib;
using System.Linq;

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
            var dirPath = Path.GetDirectoryName(outputFile);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            if (!File.Exists(outputFile))
            {
                ConvertVideo.ConvertMedia(inputFile, outputFile, "mp3");
            }
            else
            {
                MessageBox.Show("File: " + outputFile + "already exist!");
            }
        }
        public static void ConvertFileToWAV(string inputFile, int duration, string outputFile)
        {
            if (!File.Exists(inputFile))
            {
                return;
            }
            if (File.Exists(outputFile))
            {
                var player = new WindowsMediaPlayer();
                var clip = player.newMedia(outputFile).duration;
                if (duration != clip)
                {
                    File.Delete(outputFile);
                }
                else
                {
                    return;
                }
            }
            var dirPath = Path.GetDirectoryName(outputFile);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            if (!File.Exists(outputFile))
            {
                var ConvertVideo = new FFMpegConverter();
                ConvertVideo.ConvertMedia(inputFile, outputFile, "wav");
            }
        }
        public static Image GetVideoTumbnail(string inputFile, float frame, string outputPath)
        {
            if (!File.Exists(inputFile))
            {
                return null;
            }
            var spittedFiles = inputFile.Split('.');
            string outputFile = "";
            if (spittedFiles.Length == 2)
            {
                outputFile = outputPath + spittedFiles[0].Split('\\').Last() + "_" + frame + ".jpeg";
                if (File.Exists(outputFile))
                {
                    return Image.FromFile(outputFile);
                }
            }
            else
            {
                return null;
            }
            var ConvertVideo = new FFMpegConverter();
            ConvertVideo.GetVideoThumbnail(inputFile, outputFile, frame);
            return Image.FromFile(outputFile);
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

        public static Image CreateWaveImage(string path, int duration)
        {

            var myRendererSettings = new StandardWaveFormRendererSettings();
            myRendererSettings.Width = 640;
            myRendererSettings.TopHeight = 40;
            myRendererSettings.BottomHeight = 0;

            var renderer = new WaveFormRenderer();
            var image = renderer.Render(new WaveFileReader(path), myRendererSettings);

            return image;

        }
    }
}
