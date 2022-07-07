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
using System.Collections.Generic;

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
            download_bar.Maximum = _images.Length;
            downloader = new BackgroundWorker();
            downloader.WorkerReportsProgress = true;
            downloader.WorkerSupportsCancellation = true;
            downloader.DoWork += Downloader_DoWork;
            downloader.ProgressChanged += Downloader_ProgressChanged;
            downloader.RunWorkerCompleted += Downloader_RunWorkerCompleted;
            gallery_title = _title;
            this.Title = gallery_title;
            images = _images;
            save_path = _path;
            gallery_num = _gallery_num;
            Loaded += Download_Window_Loaded;
            Closed += Download_Window_Closed;
        }

        private void Download_Window_Closed(object sender, EventArgs e)
        {
            downloader.CancelAsync();
        }

        private void Downloader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Done!", "", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void Download_Window_Loaded(object sender, RoutedEventArgs e)
        {
            title.Content = $"{gallery_title}({gallery_num}) 다운로드중...";
            status.Text = "0/" + images.Length;
            downloader.RunWorkerAsync();
        }

        private void Downloader_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            download_bar.Value = e.ProgressPercentage;
        }

        private byte[] get_image(Image i)
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
                    Stream decodedImage = tools.LoadImage(imageData, ext).StreamSource;
                    using (BinaryReader br = new(decodedImage))
                    {
                        return br.ReadBytes((int)decodedImage.Length);
                    }
                }

            }
        }

        public void ArchiveFiles(string zipFileName, List<byte[]> archiveFileList)
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
                        {
                            entryStream.Write(archiveFileList[i], 0, archiveFileList[i].Length);

                        }
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            title.Content = $"{gallery_title}({gallery_num}) 압축중...";

                            status.Text = (i + 1) + "/" + images.Length;
                        }));
                        downloader.ReportProgress(i+1);
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
            List<byte[]> images_with_decoding = new();
            foreach (Image i in images)
            {
                if (downloader.CancellationPending) return;
                while (true)
                {
                    try
                    {
                        images_with_decoding.Add(get_image(i));
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            status.Text = num + "/" + images.Length;
                        }));
                        downloader.ReportProgress(num);
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
                            images_with_decoding.Add(null);
                            break;
                        }
                    }
                }
            }
            ArchiveFiles(save_path, images_with_decoding);
        }
    }
}
