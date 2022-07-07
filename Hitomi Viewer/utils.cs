using Imazen.WebP;
using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace Hitomi_Viewer
{
    internal class tools
    {
        public static BitmapImage LoadImage(byte[] imageData, string ext)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            var mem = new MemoryStream(imageData);
            mem.Position = 0;
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = null;
            if (ext == "webp")
            {
                MemoryStream decodeImage = new MemoryStream();
                SimpleDecoder dec = new SimpleDecoder();
                dec.DecodeFromBytes(imageData, imageData.Length).Save(decodeImage, ImageFormat.Png);
                image.StreamSource = decodeImage;
            }
            else if (ext == "avif")
            {
                Byte[] avifdec = Properties.Resources.avifdec;
                if(!File.Exists("avifdec.exe")) File.WriteAllBytes("avifdec.exe", avifdec);
                File.WriteAllBytes("tmp.avif", imageData);
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    FileName = "cmd.exe",
                    Arguments = "/C avifdec.exe tmp.avif tmp.png"
                };
                process.StartInfo = startInfo;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                process.WaitForExit();
                string output = process.StandardOutput.ReadToEnd();
                if (process.ExitCode == 0)
                {
                    mem = new MemoryStream(File.ReadAllBytes("tmp.png"));
                    image.StreamSource = mem;
                }
                else
                {
                    File.Delete("tmp.avif");
                    File.Delete("tmp.png");
                    throw new Exception("Failed to Load Image: The process of converting the image returned " + process.ExitCode + ".\n" + output);
                }
                File.Delete("tmp.avif");
                File.Delete("tmp.png");
            }
            else
            {
                image.StreamSource = mem;
            }
            image.EndInit();
            image.Freeze();
            return image;
        }
    }
}
