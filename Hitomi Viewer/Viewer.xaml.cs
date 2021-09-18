using Imazen.WebP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfAnimatedGif;

namespace Hitomi_Viewer
{
    /// <summary>
    /// Page1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Viewer : Page
    {
        string[] images;
        BitmapImage[] sort_images;
        string gallery_num;
        int startnum;//not index number! this is start at 1!
        int single_page_num;//not index number! this is start at 1!
        int full_page_index;
        List<List<int>> index_page;
        bool is_full = true;
        bool is_first_page;
        BackgroundWorker Image_loading;
        bool is_local;
        string zipath;

        public Viewer(string[] _images, int _startnum, string _gallery_num, bool _is_local = false)
        {
            InitializeComponent();
            is_local = _is_local;
            Loaded += Viewer_Loaded;
            if (!is_local)
            {
                images = _images;
                startnum = _startnum;
                gallery_num = _gallery_num;
                if (_startnum != 1)
                {
                    single_page_num = startnum;
                    full_page_index = (int)Math.Truncate((double)single_page_num / 2);
                }
                else
                {
                    single_page_num = startnum;
                    full_page_index = (int)Math.Truncate((double)startnum / 2);
                }
                Image_loading = new BackgroundWorker();
                Image_loading.WorkerSupportsCancellation = true;
                Image_loading.WorkerReportsProgress = true;
                is_first_page = (_startnum == 1);
                is_full = Properties.Settings.Default.is_full_page;
                Image_loading.ProgressChanged += Image_loading_ProgressChanged;
                Image_loading.DoWork += Image_loading_DoWork;
                Image_loading.RunWorkerCompleted += Image_loading_RunWorkerCompleted;
            }
            else
            {
                zipath = _images[0];
                using (ZipArchive zip = ZipFile.OpenRead(zipath))
                {
                    images = new string[zip.Entries.Count];
                }
                startnum = 1;
                single_page_num = 1;
                full_page_index = 0;
                is_first_page = (startnum == 1);
                Image_loading = new BackgroundWorker();
                Image_loading.WorkerReportsProgress = true;
                Image_loading.ProgressChanged += Image_loading_ProgressChanged;
                Image_loading.RunWorkerCompleted += Image_loading_RunWorkerCompleted;
                Image_loading.DoWork += Image_loading_DoWork;
            }
        }

        private void Image_loading_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            loading_bar.Visibility = Visibility.Hidden;
            full_page_1.Stretch = Stretch.Uniform;
            full_page_2.Stretch = Stretch.Uniform;
            single_page.Stretch = Stretch.Uniform;
            ImageBehavior.SetAnimatedSource(single_page, null);
            ImageBehavior.SetAnimatedSource(full_page_1, null);
            ImageBehavior.SetAnimatedSource(full_page_2, null);
        }

        private void Image_loading_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            loading_bar.Value = e.ProgressPercentage;
        }

        private static BitmapImage LoadImage(byte[] imageData, string ext)
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
                    mem = new MemoryStream(File.ReadAllBytes("tmp.png"));
                    image.StreamSource = mem;
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
            else
            {
                image.StreamSource = mem;
            }
            image.EndInit();
            image.Freeze();
            return image;
        }

        public static byte[] ReadToEnd(Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        private void Image_loading_DoWork(object sender, DoWorkEventArgs e)
        {
            sort_images = new BitmapImage[images.Length];

            if (is_local)
            {
                using (ZipArchive archive = ZipFile.OpenRead(zipath))
                {
                    int i = 0;
                    foreach (ZipArchiveEntry zip in archive.Entries)
                    {
                        if (Image_loading.CancellationPending == true)
                        {
                            e.Cancel = true;
                            return;
                        }
                        while (true)
                        {
                            try
                            {
                                Byte[] image = ReadToEnd(zip.Open());
                                sort_images[i] = LoadImage(image, zip.FullName.Split('.').Last());
                                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                                {
                                    if (single_page_num == 1)
                                    {
                                        ImageBehavior.SetAnimatedSource(single_page, null);
                                        single_page.Stretch = Stretch.Uniform;
                                        page_num.SelectedIndex = 0;
                                        full_page_index = 0;
                                        single_page_num = 1;
                                        single_page.Source = sort_images[0];
                                    }
                                    else
                                    {
                                        //아직 파일 로드시 북마크는 구현이 안됨

                                        if (is_full)
                                        {
                                            if (full_page_index == (int)Math.Truncate((double)i / 2))
                                            {
                                                ImageBehavior.SetAnimatedSource(full_page_1, null);
                                                ImageBehavior.SetAnimatedSource(full_page_2, null);
                                                full_page_1.Stretch = Stretch.Uniform;
                                                full_page_2.Stretch = Stretch.Uniform;
                                                full_page_1.Source = sort_images[index_page[(int)Math.Truncate((double)i / 2)][0]];
                                                full_page_2.Source = sort_images[index_page[(int)Math.Truncate((double)i / 2)][1]];
                                            }
                                        }
                                        else
                                        {
                                            if (single_page_num == i + 1)
                                            {
                                                ImageBehavior.SetAnimatedSource(single_page, null);
                                                single_page.Stretch = Stretch.Uniform;
                                                page_num.SelectedIndex = i;
                                                single_page_num = i + 1;
                                                single_page.Source = sort_images[i];
                                            }
                                        }
                                    }
                                }));
                                i++;
                                Image_loading.ReportProgress((int)Math.Round((double)(100 * (i + 1)) / images.Length));
                                break;
                            }
                            catch
                            {
                                if (MessageBox.Show((i + 1) + "번째 이미지 로딩 실패\n다시 로드하시겠습니까?", "", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                                {
                                    continue;
                                }
                                else
                                {
                                    sort_images[i] = null;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (is_first_page)
                {
                    for (int i = 0; i < images.Length; i++)
                    {
                        if (Image_loading.CancellationPending == true)
                        {
                            e.Cancel = true;
                            return;
                        }
                        while (true)
                        {
                            if (Image_loading.CancellationPending == true)
                            {
                                e.Cancel = true;
                                return;
                            }
                            try
                            {
                                using (WebClient wc = new WebClient())
                                {
                                    wc.Headers.Add("Referer", $"https://hitomi.la/reader/{gallery_num}.html");
                                    Byte[] image = wc.DownloadData(images[i]);
                                    wc.Dispose();
                                    sort_images[i] = LoadImage(image, images[i].Split('.').Last());
                                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                                    {
                                        if (single_page_num == 1)
                                        {
                                            ImageBehavior.SetAnimatedSource(single_page, null);
                                            single_page.Stretch = Stretch.Uniform;
                                            page_num.SelectedIndex = 0;
                                            full_page_index = 0;
                                            single_page_num = 1;
                                            single_page.Source = sort_images[0];
                                        }
                                        else
                                        {
                                            if (is_full)
                                            {
                                                if (full_page_index == (int)Math.Truncate((double)i / 2))
                                                {
                                                    ImageBehavior.SetAnimatedSource(full_page_1, null);
                                                    ImageBehavior.SetAnimatedSource(full_page_2, null);
                                                    full_page_1.Stretch = Stretch.Uniform;
                                                    full_page_2.Stretch = Stretch.Uniform;
                                                    full_page_1.Source = sort_images[index_page[(int)Math.Truncate((double)i / 2)][0]];
                                                    full_page_2.Source = sort_images[index_page[(int)Math.Truncate((double)i / 2)][1]];
                                                }
                                            }
                                            else
                                            {
                                                if (single_page_num == i + 1)
                                                {
                                                    ImageBehavior.SetAnimatedSource(single_page, null);
                                                    single_page.Stretch = Stretch.Uniform;
                                                    page_num.SelectedIndex = i;
                                                    single_page_num = i + 1;
                                                    single_page.Source = sort_images[i];
                                                }
                                            }
                                        }
                                    }));
                                    Image_loading.ReportProgress((int)Math.Round((double)(100 * (i + 1)) / images.Length));
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                if (MessageBox.Show((i + 1).ToString() + "번째 이미지 로드 실패.\n다시 시도하시겠습니까?\n\n" + ex.ToString(), "", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                                    continue;
                                else
                                    sort_images[i] = null;
                            }
                        }
                    }
                }
                else
                {
                    int progress = 1;
                    for (int i = index_page[(int)Math.Truncate((double)single_page_num / 2)][0]; i < images.Length; i++)
                    {
                        if (Image_loading.CancellationPending == true)
                        {
                            e.Cancel = true;
                            return;
                        }
                        while (true)
                        {
                            if (Image_loading.CancellationPending == true)
                            {
                                e.Cancel = true;
                                return;
                            }
                            try
                            {
                                using (WebClient wc = new WebClient())
                                {
                                    wc.Headers.Add("Referer", $"https://hitomi.la/reader/{gallery_num}.html");
                                    Byte[] image = wc.DownloadData(images[i]);
                                    wc.Dispose();
                                    sort_images[i] = LoadImage(image, images[i].Split('.').Last());
                                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                                    {
                                        if (single_page_num == 1)
                                        {
                                            ImageBehavior.SetAnimatedSource(single_page, null);
                                            single_page.Stretch = Stretch.Uniform;
                                            page_num.SelectedIndex = 0;
                                            full_page_index = 0;
                                            single_page_num = 1;
                                            single_page.Source = sort_images[0];
                                        }
                                        else
                                        {
                                            if (is_full)
                                            {
                                                if (full_page_index == (int)Math.Truncate((double)i / 2))
                                                {
                                                    ImageBehavior.SetAnimatedSource(full_page_1, null);
                                                    ImageBehavior.SetAnimatedSource(full_page_2, null);
                                                    full_page_1.Stretch = Stretch.Uniform;
                                                    full_page_2.Stretch = Stretch.Uniform;
                                                    full_page_1.Source = sort_images[index_page[(int)Math.Truncate((double)i / 2)][0]];
                                                    full_page_2.Source = sort_images[index_page[(int)Math.Truncate((double)i / 2)][1]];
                                                }
                                            }
                                            else
                                            {
                                                if (single_page_num == i + 1)
                                                {
                                                    ImageBehavior.SetAnimatedSource(single_page, null);
                                                    single_page.Stretch = Stretch.Uniform;
                                                    page_num.SelectedIndex = i;
                                                    single_page_num = i + 1;
                                                    single_page.Source = sort_images[i];
                                                }
                                            }
                                        }
                                    }));
                                    Image_loading.ReportProgress((int)Math.Round((double)(100 * (progress)) / images.Length));
                                    progress++;
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                if (MessageBox.Show((i + 1).ToString() + "번째 이미지 로드 실패.\n다시 시도하시겠습니까?\n\n" + ex.ToString(), "", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                                    continue;
                                else
                                    sort_images[i] = null;
                            }
                        }
                    }
                    for (int i = 0; i < images.Length; i++)
                    {
                        if (Image_loading.CancellationPending == true)
                        {
                            e.Cancel = true;
                            break;
                        }
                        if (sort_images[i] != null)
                            break;
                        while (true)
                        {
                            if (Image_loading.CancellationPending == true)
                        {
                            e.Cancel = true;
                            break;
                        }
                            try
                            {
                                using (WebClient wc = new WebClient())
                                {
                                    wc.Headers.Add("Referer", $"https://hitomi.la/reader/{gallery_num}.html");
                                    Byte[] image = wc.DownloadData(images[i]);
                                    wc.Dispose();
                                    sort_images[i] = LoadImage(image, images[i].Split('.').Last());
                                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                                    {
                                        if (single_page_num == 1)
                                        {
                                            ImageBehavior.SetAnimatedSource(single_page, null);
                                            single_page.Stretch = Stretch.Uniform;
                                            page_num.SelectedIndex = 0;
                                            full_page_index = 0;
                                            single_page_num = 1;
                                            single_page.Source = sort_images[0];
                                        }
                                        else
                                        {
                                            if (is_full)
                                            {
                                                if (full_page_index == (int)Math.Truncate((double)i / 2))
                                                {
                                                    ImageBehavior.SetAnimatedSource(full_page_1, null);
                                                    ImageBehavior.SetAnimatedSource(full_page_2, null);
                                                    full_page_1.Stretch = Stretch.Uniform;
                                                    full_page_2.Stretch = Stretch.Uniform;
                                                    full_page_1.Source = sort_images[index_page[(int)Math.Truncate((double)i / 2)][0]];
                                                    full_page_2.Source = sort_images[index_page[(int)Math.Truncate((double)i / 2)][1]];
                                                }
                                            }
                                            else
                                            {
                                                if (single_page_num == i + 1)
                                                {
                                                    ImageBehavior.SetAnimatedSource(single_page, null);
                                                    single_page.Stretch = Stretch.Uniform;
                                                    page_num.SelectedIndex = i;
                                                    single_page_num = i + 1;
                                                    single_page.Source = sort_images[i];
                                                }
                                            }
                                        }
                                    }));
                                    Image_loading.ReportProgress((int)Math.Round((double)(100 * (progress)) / images.Length));
                                    progress++;
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                if (MessageBox.Show((i + 1).ToString() + "번째 이미지 로드 실패.\n다시 시도하시겠습니까?\n\n" + ex.ToString(), "", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                                    continue;
                                else
                                    sort_images[i] = null;
                            }
                        }
                    }
                }
            }
        }

        private void Viewer_Loaded(object sender, RoutedEventArgs e)
        {
            if (is_first_page)
            {
                full.Visibility = Visibility.Hidden;
                Single.Visibility = Visibility.Visible;
                full_page_index = 0;
                page_num.SelectedIndex = 0;
            }
            else
            {
                page_num.SelectedIndex = full_page_index;
            }

            if (is_full)
            {
                if (index_page != null)
                    index_page.Clear();
                index_page = new List<List<int>>();
                page_num.Items.Clear();
                var i = 1;
                while (i <= images.Length)
                {
                    if (i == 1 || i >= images.Length)
                    {
                        page_num.Items.Add(i);
                        List<int> tmp = new List<int>();
                        tmp.Add(i - 1);
                        index_page.Add(tmp);
                        i += 1;
                    }
                    else
                    {
                        page_num.Items.Add(i + "-" + (i + 1));
                        List<int> tmp = new List<int>();
                        tmp.Add(i - 1);
                        tmp.Add(i);
                        index_page.Add(tmp);
                        i += 2;
                    }
                    if (!(i <= images.Length))
                        break;
                }
            }
            else
            {
                page_num.Items.Clear();
                for (int i = 1; i < images.Length + 1; i++)
                {
                    page_num.Items.Add(i.ToString());
                }
            }
            Image_loading.RunWorkerAsync();
        }

        private void is_full_spread_Checked(object sender, RoutedEventArgs e)
        {
            is_full = true;
            Properties.Settings.Default.is_full_page = true;
            Properties.Settings.Default.Save();
            if (page_num == null || single_page == null || full == null)
                return;
            if (single_page_num != 1)
            {
                Single.Visibility = Visibility.Hidden;
                full.Visibility = Visibility.Visible;
            }
            else
            {
                Single.Visibility = Visibility.Visible;
                full.Visibility = Visibility.Hidden;
            }
            if (index_page != null)
                index_page.Clear();
            index_page = new List<List<int>>();
            page_num.Items.Clear();
            var i = 1;
            while (i <= images.Length)
            {
                if (i == 1 || i >= images.Length)
                {
                    page_num.Items.Add(i);
                    List<int> tmp = new List<int>();
                    tmp.Add(i - 1);
                    index_page.Add(tmp);
                    i += 1;
                }
                else
                {
                    page_num.Items.Add(i + "-" + (i + 1));
                    List<int> tmp = new List<int>();
                    tmp.Add(i - 1);
                    tmp.Add(i);
                    index_page.Add(tmp);
                    i += 2;
                }
                if (!(i <= images.Length))
                    break;
            }
            full_page_index = (int)Math.Truncate((double)single_page_num / 2);
            page_num.SelectedIndex = full_page_index;
            List<int> _images = index_page[full_page_index];
            if (_images.Count == 1)
            {
                full.Visibility = Visibility.Hidden;
                Single.Visibility = Visibility.Visible;
                single_page.Source = sort_images[_images[0]];
            }
            else
            {
                Single.Visibility = Visibility.Hidden;
                full.Visibility = Visibility.Visible;
                full_page_1.Source = sort_images[_images[0]];
                full_page_2.Source = sort_images[_images[1]];
            }
        }

        private void is_full_spread_Unchecked(object sender, RoutedEventArgs e)
        {
            is_full = false;
            Properties.Settings.Default.is_full_page = false;
            Properties.Settings.Default.Save();
            if (page_num == null || single_page == null || full == null)
                return;
            full.Visibility = Visibility.Hidden;
            Single.Visibility = Visibility.Visible;
            page_num.Items.Clear();
            for (int i = 1; i < images.Length + 1; i++)
            {
                page_num.Items.Add(i.ToString());
            }
            /*if (sort_images.Count != images.Length)
                return;*/
            if (single_page_num <= 0)
                single_page_num = 1;
            page_num.SelectedIndex = single_page_num - 1;
        }

        private void page_num_DataContextChanged(object sender, SelectionChangedEventArgs e)
        {
            if (is_full)
            {
                if (page_num.SelectedIndex == -1 || index_page.Count != page_num.Items.Count)
                    return;
                full_page_index = page_num.SelectedIndex;
                single_page_num = full_page_index * 2;
                List<int> _images = index_page[full_page_index];
                if (_images.Count == 1)
                {
                    try
                    {
                        ImageBehavior.SetAnimatedSource(single_page, null);
                        full.Visibility = Visibility.Hidden;
                        Single.Visibility = Visibility.Visible;
                        if (sort_images[_images[0]] == null)
                            throw new Exception("image not loading");
                        single_page.Stretch = Stretch.Uniform;
                        single_page.Source = sort_images[_images[0]];
                    }
                    catch
                    {
                        single_page.Stretch = Stretch.None;
                        ImageBehavior.SetAnimatedSource(single_page, new BitmapImage(new Uri("pack://siteoforigin:,,,/Resources/Loading.gif")));
                    }
                }
                else
                {
                    try
                    {
                        ImageBehavior.SetAnimatedSource(full_page_1, null);
                        ImageBehavior.SetAnimatedSource(full_page_2, null);
                        Single.Visibility = Visibility.Hidden;
                        full.Visibility = Visibility.Visible;
                        if (sort_images[_images[0]] == null || sort_images[_images[1]] == null)
                            throw new Exception("image not loading");
                        full_page_1.Stretch = Stretch.Uniform;
                        full_page_2.Stretch = Stretch.Uniform;
                        full_page_1.Source = sort_images[_images[0]];
                        full_page_2.Source = sort_images[_images[1]];
                    }
                    catch
                    {
                        full_page_1.Stretch = Stretch.None;
                        full_page_2.Stretch = Stretch.None;
                        ImageBehavior.SetAnimatedSource(full_page_1, new BitmapImage(new Uri("pack://siteoforigin:,,,/Resources/Loading.gif")));
                        ImageBehavior.SetAnimatedSource(full_page_2, new BitmapImage(new Uri("pack://siteoforigin:,,,/Resources/Loading.gif")));
                    }
                }
            }
            else
            {
                if (page_num.SelectedIndex == -1)
                {
                    return;
                }
                single_page_num = page_num.SelectedIndex + 1;
                full_page_index = (int)Math.Truncate((double)single_page_num / 2);
                try
                {
                    ImageBehavior.SetAnimatedSource(single_page, null);
                    if (sort_images[single_page_num - 1] == null)
                        throw new Exception("image not loading");
                    single_page.Stretch = Stretch.Uniform;
                    single_page.Source = sort_images[single_page_num - 1];
                }
                catch
                {
                    single_page.Stretch = Stretch.None;
                    ImageBehavior.SetAnimatedSource(single_page, new BitmapImage(new Uri("pack://siteoforigin:,,,/Resources/Loading.gif")));
                }
            }
        }

        private void full_page_1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(full_page_index - 1 > index_page.Count) && full_page_index - 1 >= 0 && single_page_num - 1 >= 0)
            {
                single_page_num -= 2;
                full_page_index--;
                page_num.SelectedIndex = full_page_index;
            }
        }

        private void full_page_2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(full_page_index + 1 > index_page.Count) && full_page_index + 1 >= 0 && single_page_num + 1 <= images.Length)
            {
                single_page_num += 2;
                full_page_index++;
                page_num.SelectedIndex = full_page_index;
            }
        }

        private void single_page_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!Convert.ToBoolean(is_full_spread.IsChecked) && single_page_num + 1 <= sort_images.Length && single_page_num + 1 > 0)
            {
                single_page_num++;
                full_page_index = (int)Math.Truncate((double)single_page_num / 2);
                page_num.SelectedIndex = single_page_num - 1;
            }
            else if (Convert.ToBoolean(is_full_spread.IsChecked))
            {
                full_page_index++;
                single_page_num += 2;
                page_num.SelectedIndex = full_page_index;
            }
        }

        private void goto_search_Click(object sender, RoutedEventArgs e)
        {
            Image_loading.Dispose();
            NavigationService.Navigate(new Load_info(), UriKind.Relative);
        }

        private void Page_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.NumPad6 || e.Key == Key.D)
            {
                if (is_full)
                {
                    if (!(full_page_index + 1 > index_page.Count) && full_page_index + 1 >= 0 && single_page_num + 1 <= images.Length)
                    {
                        single_page_num += 2;
                        full_page_index++;
                        page_num.SelectedIndex = full_page_index;
                    }
                }
                else
                {
                    if (!Convert.ToBoolean(is_full_spread.IsChecked) && single_page_num + 1 <= sort_images.Length && single_page_num + 1 > 0)
                    {
                        single_page_num++;
                        full_page_index = (int)Math.Truncate((double)single_page_num / 2);
                        page_num.SelectedIndex = single_page_num - 1;
                    }
                }
                
            }else if(e.Key == Key.NumPad4 || e.Key == Key.A)
            {
                if (is_full)
                {
                    if (!(full_page_index - 1 > index_page.Count) && full_page_index - 1 >= 0 && single_page_num - 1 >= 0)
                    {
                        single_page_num -= 2;
                        full_page_index--;
                        page_num.SelectedIndex = full_page_index;
                    }
                }
                else
                {
                    if (!Convert.ToBoolean(is_full_spread.IsChecked) && single_page_num + 1 <= sort_images.Length && single_page_num + 1 > 0)
                    {
                        single_page_num--;
                        full_page_index = (int)Math.Truncate((double)single_page_num / 2);
                        page_num.SelectedIndex = single_page_num - 1;
                    }
                }
            }else if(e.Key == Key.NumPad5 || e.Key == Key.S)
            {
                single_page_num = 1;
                full_page_index = 0;
                page_num.SelectedIndex = full_page_index;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.KeyDown += Page_PreviewKeyDown;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i< sort_images.Length; i++)
            {
                sort_images[i] = null;
            }
        }
    }
}