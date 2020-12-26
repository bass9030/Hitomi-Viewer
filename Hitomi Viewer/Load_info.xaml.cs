using Hitomi_Core;
using Imazen.WebP;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Security.Cryptography;

namespace Hitomi_Viewer
{
    /// <summary>
    /// Load_info.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Load_info : Page
    {
        BackgroundWorker get_info;
        string gallery_num;
        Hitomi hitomi;
        JObject bookmark;
        private static readonly Regex _regex = new Regex("[^0-9]+"); //regex that matches disallowed text

        public Load_info()
        {
            InitializeComponent();
            get_info = new BackgroundWorker();
            get_info.DoWork += Get_info_DoWork;
            try
            {
                if (Properties.Settings.Default.bookmark == "")
                {
                    bookmark = new JObject();
                }
                else
                {
                    bookmark = JObject.Parse(Properties.Settings.Default.bookmark);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("저장된 설정값을 불러올 수 없습니다.\n이전에 설정한 값들이 초기화되거나 이전으로 돌아갈 수 있습니다.\n\n" + ex.ToString(), "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void Load_at_page_num_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bookmark[keyword.Text] = Load_at_page_num.SelectedIndex + 1;
            Properties.Settings.Default.bookmark = bookmark.ToString();
            Properties.Settings.Default.Save();
            Console.WriteLine(bookmark);
        }

        private void keyword_PreviewKeyDown(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void keyword_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void search_Click(object sender, RoutedEventArgs e)
        {
            gallery_num = keyword.Text;
            search.IsEnabled = false;
            keyword.IsEnabled = false;
            get_info.RunWorkerAsync();
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
                dec.DecodeFromBytes(imageData, imageData.Length).Save(mem, System.Drawing.Imaging.ImageFormat.Png);
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


        private void Get_info_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                hitomi = new Hitomi(gallery_num);
                string[] images = hitomi.images;
                string thumbnail_image = images[0];
                if (thumbnail_image == null)
                {
                    MessageBox.Show("썸네일 이미지를 받아오지 못했습니다.", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        search.IsEnabled = true;
                        keyword.IsEnabled = true;
                    }));
                    return;
                }
                bool iserr = false;
                string errmsg = "";
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("Referer", "https://hitomi.la/reader/" + gallery_num + ".html");
                    Byte[] Mydata = wc.DownloadData(thumbnail_image);
                    string subtitles = "Artist: " + hitomi.Artist + "(" + hitomi.Group + ")\n" +
                        "Series: " + hitomi.Series + "\n" +
                        "Type: " + hitomi.type + "\n" +
                        "Language: " + hitomi.language + "\n" +
                        "Tags: " + string.Join(", ", hitomi.tags) + "\n" +
                        "Upload date: " + hitomi.upload_date;
                    wc.Dispose();
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        try
                        {
                            thumbnail.Source = LoadImage(Mydata, thumbnail_image.Split('.')[thumbnail_image.Split('.').Length - 1]);
                            for(int i = 1; i < images.Length + 1; i++)
                            {
                                Load_at_page_num.Items.Add(i.ToString());
                            }
                            title.Content = hitomi.title;
                            subtitle.Content = subtitles;
                            View.Visibility = Visibility.Visible;
                            Download.Visibility = Visibility.Visible;
                            Load_at_page.Visibility = Visibility.Visible;
                            Load_at_page_num.Visibility = Visibility.Visible;
                            Load_at_page_num.IsEnabled = false;
                            search.IsEnabled = true;
                            keyword.IsEnabled = true;
                        }
                        catch (Exception ex)
                        {
                            iserr = true;
                            errmsg = ex.ToString();
                        }
                    }));
                }
                if (iserr)
                    throw new Exception(errmsg);

                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    try
                    {
                        if (bookmark[hitomi.gallery_id] != null && bookmark.Value<int>(hitomi.gallery_id) != 0)
                        {
                            Load_at_page.IsChecked = true;
                            Load_at_page_num.SelectedIndex = bookmark.Value<int>(hitomi.gallery_id) - 1;
                        }
                    }
                    catch
                    {
                        bookmark.Property(gallery_num).Remove();
                        MessageBox.Show("북마크 로드 실패", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }));
            }
            catch(Exception ex)
            {
                MessageBox.Show("작품 정보를 불러오지 못하였습니다.\n\n" + ex.ToString(), "", MessageBoxButton.OK, MessageBoxImage.Error);
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    search.IsEnabled = true;
                    keyword.IsEnabled = true;
                }));
                return;
            }
        }

        private void View_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Viewer(hitomi.images, (Convert.ToBoolean(Load_at_page.IsChecked) ? Convert.ToInt32(Load_at_page_num.SelectedItem) : 1), gallery_num), UriKind.Relative);
        }

        private void Load_at_page_Checked(object sender, RoutedEventArgs e)
        {
            Load_at_page_num.IsEnabled = true;
            Console.WriteLine(bookmark);
        }

        private void Load_at_page_Unchecked(object sender, RoutedEventArgs e)
        {
            Load_at_page_num.IsEnabled = false;
            bookmark.Property(gallery_num).Remove();
            Load_at_page_num.SelectedIndex = -1;
            Console.WriteLine(bookmark);
            Properties.Settings.Default.bookmark = bookmark.ToString();
            Properties.Settings.Default.Save();
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog diglog = new System.Windows.Forms.FolderBrowserDialog();
            diglog.RootFolder = Environment.SpecialFolder.Desktop;
            diglog.Description = "저장할 폴더 선택...";
            if(diglog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Download_Window down = new Download_Window(hitomi.title, hitomi.images, diglog.SelectedPath, gallery_num);
                down.ShowActivated = true;
                down.Show();
            }
        }

        private void Load_at_file_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog diglog = new OpenFileDialog();
            diglog.Filter = "압축 파일 (*.zip)|*.zip";
            diglog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            diglog.Multiselect = false;
            diglog.Title = "파일 선택...";
            if (diglog.ShowDialog() == true)
            {
                Viewer viewer = new Viewer(new string[] { diglog.FileName }, 1, null, true);
                NavigationService.Navigate(viewer, UriKind.Relative);
            }
        }

        private void Reset_settings_file_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("정말로 설정파일을 초기화 하시겠습니까?\n초기화시 북마크와 풀페이지 모드 여부가 기본값으로 돌아갑니다.", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;
            Properties.Settings.Default.is_full_page = true;
            Properties.Settings.Default.bookmark = "";
            Properties.Settings.Default.Save();
            MessageBox.Show("완료되었습니다.", "", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
