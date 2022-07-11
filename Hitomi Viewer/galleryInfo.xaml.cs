using cs_hitomi;
using Imazen.WebP;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
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

namespace Hitomi_Viewer
{
    public delegate void onViewClickHandler(int startPage, Gallery gallery);

    /// <summary>
    /// galleryInfo.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class galleryInfo : UserControl
    {
        utils.ImageUrlResolver imgResolver;
        int galleryNumber;
        JObject bookmark;
        Gallery gallery;
        public onViewClickHandler onViewClick;
        public EventHandler onLoaded;
        bool safeMode = false;
        BackgroundWorker worker = new();

        public galleryInfo(int _galleryNumber, bool safeMode = false)
        {
            galleryNumber = _galleryNumber;
            this.safeMode = safeMode;
            /*try
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
            catch (Exception ex)
            {
                MessageBox.Show("저장된 설정값을 불러올 수 없습니다.\n이전에 설정한 값들이 초기화되거나 이전으로 돌아갈 수 있습니다.\n\n" + ex.ToString(), "", MessageBoxButton.OK, MessageBoxImage.Error);
            }*/
            InitializeComponent();
            worker.DoWork += GalleryInfo_Loaded;
            worker.RunWorkerAsync();
            Unloaded += GalleryInfo_Unloaded;
            //GalleryInfo_Loaded(null, null);
        }

        private void GalleryInfo_Unloaded(object sender, RoutedEventArgs e)
        {
            thumbnail.Source = null;
            GC.Collect();
        }

        private void View_Click(object sender, RoutedEventArgs e)
        {
            onViewClick?.Invoke(Convert.ToBoolean(Load_at_page.IsChecked) ? Convert.ToInt32(Load_at_page_num.SelectedItem) : 1, gallery);
        }

        private void Load_at_page_Checked(object sender, RoutedEventArgs e)
        {
            Load_at_page_num.IsEnabled = true;
            Console.WriteLine(bookmark);
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog diglog = new();
            diglog.DefaultExt = ".zip";
            diglog.Filter = "Zip file (*.zip)|*.zip";
            diglog.FileName = $"{Regex.Replace(gallery.title.display, "[\\\\/:*?\"<>|]", "_")}({galleryNumber})";
            diglog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (diglog.ShowDialog() == true)
            {
                Download_Window down = new Download_Window(gallery.title.display, gallery.files, diglog.FileName, galleryNumber.ToString());
                down.ShowActivated = true;
                down.Show();
            }
        }

        private void Load_at_page_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                Load_at_page_num.IsEnabled = false;
                if (bookmark.Property(galleryNumber.ToString()) != null) bookmark.Property(galleryNumber.ToString()).Remove();
                Load_at_page_num.SelectedIndex = -1;
                Console.WriteLine(bookmark);
                Properties.Settings.Default.bookmark = bookmark.ToString();
                Properties.Settings.Default.Save();
            }
            catch { }
        }

        

        private void Load_at_page_num_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bookmark[galleryNumber] = Load_at_page_num.SelectedIndex + 1;
            Properties.Settings.Default.bookmark = bookmark.ToString();
            Properties.Settings.Default.Save();
            Console.WriteLine(bookmark);
        }


        //INFO: THIS FUNCTION IS ASYNC
        //      NOT LOADED EVENT FUNCTION
        private void GalleryInfo_Loaded(object sender, DoWorkEventArgs e)
        {
            //if (safeMode) Dispatcher.Invoke(() => thumbnail.Visibility = Visibility.Hidden);
            while (imgResolver == null)
            {
                try
                {
                    imgResolver = new();
                    break;
                }
                catch
                {
                    /*if (MessageBox.Show("이미지 불러오기 실패!\n다시 시도하시겠습니까?", "", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.No)
                    {
                        return;
                    }*/
                }
            }
            while(true)
            {
                try
                {
                    gallery = utils.getGallery(galleryNumber, true);
                    break;
                }
                catch { }
            }
            cs_hitomi.Image thumbnail_image = gallery.files[0];
            List<string> tags = new List<string>();
            foreach (Tag tag in gallery.tags) tags.Add(tag.ToString());

            /*string subtitles = "Artist: " + (gallery.artists.Length != 0 ? string.Join(", ", gallery.artists) : "N/A") + (gallery.groups.Length != 0 ? "(" + string.Join(", ", gallery.groups) + ")" : "") + "\n" +
                ((gallery.series.Length != 0) ? "Series: " + string.Join(", ", gallery.series) + "\n" : "") +
                "Type: " + gallery.type + "\n" +
                "Language: " + gallery.languageName + "\n" +
                "Tags: " + string.Join(", ", tags) + "\n" +
                "Upload date: " + gallery.publishedDate.ToString();*/

            BitmapImage thumb;
            while(true)
            {
                try
                {
                    using (HttpClient wc = new())
                    {
                        wc.DefaultRequestHeaders.Add("Referer", "https://hitomi.la");
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, imgResolver.getImageUrl(gallery.files[0], gallery.files[0].hasWebp ? "webp" : "avif", true));
                        using (MemoryStream ms = new MemoryStream())
                        {
                            wc.Send(request).Content.ReadAsStream().CopyTo(ms);
                            thumb = tools.LoadImage(ms.ToArray(), gallery.files[0].hasWebp ? "webp" : "avif");
                            break;
                        }
                    }
                }
                catch(Exception ex) { Console.WriteLine(ex); }
            }

            List<string> page = new List<string>();

            for (int i = 1; i < gallery.files.Length + 1; i++)
            {
                page.Add(i.ToString());
            }

            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                thumbnail.Source = thumb;
                Load_at_page_num.ItemsSource = page;
                title.Text = gallery.title.display;
                info.Children.Add(
                    new galleryDetails(gallery.artists, 
                    gallery.groups, 
                    gallery.series, 
                    gallery.type.ToString(), 
                    gallery.languageName.ToString(), 
                    gallery.tags, 
                    gallery.publishedDate
                ));
                View.Visibility = Visibility.Visible;
                Download.Visibility = Visibility.Visible;
                Load_at_page.Visibility = Visibility.Visible;
                Load_at_page_num.Visibility = Visibility.Visible;
                Load_at_page_num.IsEnabled = false;

                loading.Visibility = Visibility.Hidden;
                content.Visibility = Visibility.Visible;
                
                /*try
                {
                    if (bookmark[gallery.id] != null && bookmark.Value<int>(gallery.id) != 0)
                    {
                        Load_at_page.IsChecked = true;
                        Load_at_page_num.SelectedIndex = bookmark.Value<int>(gallery.id) - 1;
                    }
                }
                catch
                {
                    //bookmark.Property(gallery_num).Remove();
                    MessageBox.Show("북마크 로드 실패", "", MessageBoxButton.OK, MessageBoxImage.Error);
                }*/
            }));


            onLoaded?.Invoke(this, EventArgs.Empty);
        }
    }
}
