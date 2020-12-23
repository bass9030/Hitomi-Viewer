using Imazen.WebP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
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
        List<BitmapImage> sort_images;
        string gallery_num;
        int startnum;//not index number! this is start at 1!
        int single_page_num;//not index number! this is start at 1!
        int full_page_index;
        List<List<int>> index_page;
        bool is_full = true;
        bool is_first_page;
        BackgroundWorker Image_loading;

        public Viewer(string[] _images, int _startnum, string _gallery_num)
        {
            InitializeComponent();
            images = _images;
            startnum = _startnum;
            gallery_num = _gallery_num;
            single_page_num = startnum;
            full_page_index = (int)Math.Truncate((double)startnum / 2);
            if(_startnum != 1)
            {
                single_page_num = startnum;
                full_page_index = (int)Math.Truncate((double)single_page_num / 2);
            }
            Image_loading = new BackgroundWorker();
            Image_loading.WorkerReportsProgress = true;
            is_first_page = (_startnum == 1);
            is_full = Properties.Settings.Default.is_full_page;
            Image_loading.ProgressChanged += Image_loading_ProgressChanged;
            Image_loading.DoWork += Image_loading_DoWork;
            Image_loading.RunWorkerCompleted += Image_loading_RunWorkerCompleted;
            Loaded += Viewer_Loaded;
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
                SimpleDecoder dec = new SimpleDecoder();
                dec.DecodeFromBytes(imageData, imageData.Length).Save(mem, ImageFormat.Png);
                image.StreamSource = mem;
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

        private void Image_loading_DoWork(object sender, DoWorkEventArgs e)
        {
            sort_images = new List<BitmapImage>();
            for (int i = 0; i < images.Length; i++)
            {
                while (true)
                {
                    try
                    {
                        using (WebClient wc = new WebClient())
                        {
                            wc.Headers.Add("Referer", $"https://hitomi.la/reader/{gallery_num}.html");
                            Byte[] image = wc.DownloadData(images[i]);
                            wc.Dispose();
                            sort_images.Add(LoadImage(image, images[i].Split('.').Last()));
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
                    catch(Exception ex)
                    {
                        if (MessageBox.Show((i + 1).ToString() + "번째 이미지 다운로드 실패.\n다시 시도하시겠습니까?\n\n" + ex.ToString(), "", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                            continue;
                        else
                        {
                            sort_images.Add(null);
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
            Console.WriteLine(single_page_num);
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
                    Console.WriteLine(1);
                    try
                    {
                        ImageBehavior.SetAnimatedSource(single_page, null);
                        full.Visibility = Visibility.Hidden;
                        Single.Visibility = Visibility.Visible;
                        single_page.Stretch = Stretch.Uniform;
                        single_page.Source = sort_images[_images[0]];
                        Console.WriteLine("loading done.");
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        Console.WriteLine("loading not done.");
                        single_page.Stretch = Stretch.None;
                        ImageBehavior.SetAnimatedSource(single_page, new BitmapImage(new Uri("pack://siteoforigin:,,,/Resources/Loading.gif")));
                    }
                }
                else
                {
                    Console.WriteLine(2);
                    try
                    {
                        ImageBehavior.SetAnimatedSource(full_page_1, null);
                        ImageBehavior.SetAnimatedSource(full_page_2, null);
                        Single.Visibility = Visibility.Hidden;
                        full.Visibility = Visibility.Visible;
                        full_page_1.Stretch = Stretch.Uniform;
                        full_page_2.Stretch = Stretch.Uniform;
                        full_page_1.Source = sort_images[_images[0]];
                        full_page_2.Source = sort_images[_images[1]];
                        Console.WriteLine("loading done.");
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        Console.WriteLine("loading not done.");
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
                    Console.WriteLine("return");
                    return;
                }
                single_page_num = page_num.SelectedIndex + 1;
                full_page_index = (int)Math.Truncate((double)single_page_num / 2);
                try
                {
                    ImageBehavior.SetAnimatedSource(single_page, null);
                    single_page.Stretch = Stretch.Uniform;
                    single_page.Source = sort_images[single_page_num - 1];
                }
                catch(ArgumentOutOfRangeException)
                {
                    single_page.Stretch = Stretch.None;
                    ImageBehavior.SetAnimatedSource(single_page, new BitmapImage(new Uri("pack://siteoforigin:,,,/Resources/Loading.gif")));
                }
            }
            Console.WriteLine($"single_page_num: {single_page_num}");
            Console.WriteLine($"full_page_index: {full_page_index}");
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
            if (!Convert.ToBoolean(is_full_spread.IsChecked) && single_page_num + 1 <= sort_images.Count && single_page_num + 1 > 0)
            {
                single_page_num++;
                full_page_index = (int)Math.Truncate((double)single_page_num / 2);
                page_num.SelectedIndex = single_page_num - 1;
            }
            else if(Convert.ToBoolean(is_full_spread.IsChecked))
            {
                full_page_index++;
                single_page_num += 2;
                page_num.SelectedIndex = full_page_index;
            }
        }
    }
}