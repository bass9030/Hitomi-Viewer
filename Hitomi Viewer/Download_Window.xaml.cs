using System;
using System.ComponentModel;
using System.Windows;
using System.IO.Compression;
using System.Net;
using System.IO;
using Imazen.WebP;
using System.Windows.Threading;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Text.RegularExpressions;
using cs_hitomi;
using System.Net.Http;

namespace Hitomi_Viewer
{
    /// <summary>
    /// Download_Window.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Download_Window : Window
    {
        BackgroundWorker downloader;
        string gallery_title;
        utils.ImageUrlResolver urlResolver = new utils.ImageUrlResolver();
        Image[] images;
        string save_path;
        string gallery_num;

        public Download_Window(string _title, Image[] _images, string _path, string _gallery_num)
        {
            InitializeComponent();
            downloader = new BackgroundWorker();
            downloader.WorkerReportsProgress = true;
            downloader.DoWork += Downloader_DoWork;
            downloader.ProgressChanged += Downloader_ProgressChanged;
            downloader.RunWorkerCompleted += Downloader_RunWorkerCompleted;
            gallery_title = _title;
            this.Title = gallery_title;
            images = _images;
            save_path = _path;
            gallery_num = _gallery_num;
            Loaded += Download_Window_Loaded;
        }

        private void Downloader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Done!", "", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void Download_Window_Loaded(object sender, RoutedEventArgs e)
        {
            title.Content = "Downloading " + gallery_title + "...";
            status.Text = "0/" + images.Length;
            downloader.RunWorkerAsync();
        }

        private void Downloader_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            download_bar.Value = e.ProgressPercentage;
        }

        private MemoryStream get_image(Image i)
        {
            string ext = i.hasWebp ? "webp" : "avif";
            using(HttpClient wc = new HttpClient())
            {
                wc.DefaultRequestHeaders.Add("Referer", "https://hitomi.la");
                HttpRequestMessage request = new(HttpMethod.Get, urlResolver.getImageUrl(i, ext));
                using (MemoryStream mem = new MemoryStream())
                {
                    wc.Send(request).Content.ReadAsStream().CopyTo(mem);
                    byte[] imageData = mem.ToArray();
                    mem.Position = 0;
                    if (ext == "webp")
                    {
                        SimpleDecoder dec = new SimpleDecoder();
                        dec.DecodeFromBytes(imageData, imageData.Length).Save(mem, ImageFormat.Png);
                    }
                    else if (ext == "avif")
                    {
                        Byte[] avifdec = Properties.Resources.avifdec;
                        File.WriteAllBytes("avifdec.exe", avifdec);
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
                            mem.Write(File.ReadAllBytes("tmp.png"), 0, File.ReadAllBytes("tmp.png").Length);
                        }
                        else
                        {
                            File.Delete("tmp.avif");
                            File.Delete("tmp.png");
                            File.Delete("avifdec.exe");
                            throw new Exception("Failed to Load Image: The process of converting the image returned " + process.ExitCode + ".\n" + output);
                        }
                        File.Delete("tmp.avif");
                        File.Delete("tmp.png");
                        File.Delete("avifdec.exe");
                    }

                    return mem;
                }

            }
        }

        public void ArchiveFiles(string zipFileName, MemoryStream[] archiveFileList)
        {
            using (var ms = new MemoryStream())
            {
                // archive log files
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    int i = 0;
                    foreach (var f in archiveFileList)
                    {
                        if (f == null)
                            continue;
                        var zipEntity = archive.CreateEntry((i + 1) + ".png");

                        using (var entryStream = zipEntity.Open())
                        using (var readStream = archiveFileList[i])
                        {
                            readStream.CopyTo(entryStream);
                        }
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            status.Text = "Compressing... " + (i + 1) + "/" + images.Length;
                        }));
                        downloader.ReportProgress((int)Math.Round((double)(100 * (i + 1)) / images.Length));
                        i++;
                    }
                }

                // write archived data
                using (var fs = new FileStream(zipFileName, FileMode.Create))
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.CopyTo(fs);
                }
            }
        }

        private void Downloader_DoWork(object sender, DoWorkEventArgs e)
        {
            int num = 1;
            MemoryStream[] images_with_decoding = new MemoryStream[images.Length];
            foreach (Image i in images)
            {
                while (true)
                {
                    try
                    {
                        images_with_decoding[num - 1] = get_image(i);
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            status.Text = "Downloading... " + num + "/" + images.Length;
                        }));
                        downloader.ReportProgress((int)Math.Round((double)(100 * num) / images.Length));
                        num++;
                        break;
                    }
                    catch(Exception ex)
                    {
                        if(MessageBox.Show(num + "번째 이미지 다운로드 실패!\n다시 시도하시겠습니까?\n\n" + ex.ToString(), "", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                        {
                            continue;
                        }
                        else
                        {
                            images_with_decoding[num - 1] = null;
                            break;
                        }
                    }
                }
            }
            ArchiveFiles(save_path + "\\" + Regex.Replace(gallery_title, "[\\\\/:*?\"<>|]", "_") + "(" + gallery_num + ").zip", images_with_decoding);
        }
    }
}
