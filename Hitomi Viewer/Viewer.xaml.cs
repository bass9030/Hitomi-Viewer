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
using cs_hitomi;
using System.Net.Http;

namespace Hitomi_Viewer
{
    /// <summary>
    /// Page1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Viewer : Page
    {
        cs_hitomi.Image[] images;
        string[] images_local;
        BitmapImage[] sort_images;
        utils.ImageUrlResolver UrlResolver;
        string gallery_num;
        int startnum; // WARING: NOT INDEX NUMBER! it is start at 1!
        int single_page_num; // WARING: NOT INDEX NUMBER! it is start at 1!
        int full_page_index;
        List<List<int>> index_page;
        bool is_full = true;
        bool is_first_page;
        BackgroundWorker Image_loading;
        bool is_local;
        string zipath;

        public Viewer(cs_hitomi.Image[] _images, int _startnum, string _gallery_num)
        {
            UrlResolver = new utils.ImageUrlResolver();
            InitializeComponent();
            Loaded += Viewer_Loaded;

            Unloaded += Viewer_Unloaded;

            images = _images;
            startnum = _startnum;
            gallery_num = _gallery_num;
            single_page_num = startnum;
            loading_bar.Maximum = _images.Length;

            if (_startnum != 1)
                full_page_index = (int)Math.Truncate((double)single_page_num / 2);
            else
                full_page_index = (int)Math.Truncate((double)startnum / 2);

            Image_loading = new BackgroundWorker();
            Image_loading.WorkerSupportsCancellation = true;
            Image_loading.WorkerReportsProgress = true;
            is_first_page = (_startnum == 1);
            is_full = Properties.Settings.Default.is_full_page;
            Image_loading.ProgressChanged += Image_loading_ProgressChanged;
            Image_loading.DoWork += Image_loading_DoWork;
            Image_loading.RunWorkerCompleted += Image_loading_RunWorkerCompleted;
        }

        private void Viewer_Unloaded(object sender, RoutedEventArgs e)
        {
            single_page.Source = null;
            full_page_1.Source = null;
            full_page_2.Source = null;
            this.PreviewKeyDown -= Page_PreviewKeyDown;
            sort_images = null;
            GC.Collect();
        }

        public Viewer(string[] _images, int _startnum, string _gallery_num)
        {
            InitializeComponent();
            Loaded += Viewer_Loaded;

            zipath = _images[0];
            using (ZipArchive zip = ZipFile.OpenRead(zipath))
            {
                images_local = new string[zip.Entries.Count];
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

        private void loadImageOnControl(int index)
        {
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
                        if (full_page_index == (int)Math.Truncate((double)index / 2))
                        {
                            ImageBehavior.SetAnimatedSource(full_page_1, null);
                            ImageBehavior.SetAnimatedSource(full_page_2, null);
                            full_page_1.Stretch = Stretch.Uniform;
                            full_page_2.Stretch = Stretch.Uniform;
                            full_page_1.Source = sort_images[index_page[(int)Math.Truncate((double)index / 2)][0]];
                            full_page_2.Source = sort_images[index_page[(int)Math.Truncate((double)index / 2)][1]];
                        }
                    }
                    else
                    {
                        if (single_page_num == index + 1)
                        {
                            ImageBehavior.SetAnimatedSource(single_page, null);
                            single_page.Stretch = Stretch.Uniform;
                            page_num.SelectedIndex = index;
                            single_page_num = index + 1;
                            single_page.Source = sort_images[index];
                        }
                    }
                }
            }));
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
                        if (Image_loading.CancellationPending) break;

                        while (!Image_loading.CancellationPending)
                        {
                            try
                            {
                                Byte[] image = ReadToEnd(zip.Open());
                                sort_images[i] = tools.LoadImage(image, zip.FullName.Split('.').Last());
                                loadImageOnControl(i);
                                i++;
                                Image_loading.ReportProgress(i+1);
                                break;
                            }
                            catch
                            {
                                if (MessageBox.Show((i + 1) + "번째 이미지 로딩 실패\n다시 로드하시겠습니까?", "", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                                    continue;
                                else
                                    sort_images[i] = null;
                            }
                        }
                    }
                }
            }
            else
            {
                int progress = 1;

                for (int i = (is_first_page ? 0 : index_page[(int)Math.Truncate((double)single_page_num / 2)][0]); (is_first_page ? i < images.Length : progress - 1 < images.Length); i++)
                {
                    if (i > images.Length - 1) i = 0;
                    if (Image_loading.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }
                    while (!Image_loading.CancellationPending)
                    {
                        try
                        {
                            using (HttpClient wc = new())
                            {
                                wc.DefaultRequestHeaders.Add("Referer", $"https://hitomi.la");
                                HttpRequestMessage request = new(HttpMethod.Get, UrlResolver.getImageUrl(images[i], images[i].hasWebp ? "webp" : "avif"));
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    wc.Send(request).Content.ReadAsStream().CopyTo(ms);
                                    sort_images[i] = tools.LoadImage(ms.ToArray(), images[i].hasWebp ? "webp" : "avif");
                                }
                                loadImageOnControl(i);
                                Image_loading.ReportProgress(i+1);
                                progress++;
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (Image_loading.CancellationPending)
                            {
                                e.Cancel = true;
                                break;
                            }
                            if (MessageBox.Show((i + 1).ToString() + "번째 이미지 로드 실패.\n다시 시도하시겠습니까?\n\n" + ex.ToString(), "", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                                continue;
                            else
                                sort_images[i] = null;
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
            Image_loading.CancelAsync();
            //Image_loading.Dispose();
            //sort_images = null;
            NavigationService.GoBack();
            //NavigationService.Navigate(new Load_info(), UriKind.Relative);
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
    }
}