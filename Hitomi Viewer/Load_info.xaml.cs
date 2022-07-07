using cs_hitomi;
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
using System.Net.Http;

namespace Hitomi_Viewer
{
    /// <summary>
    /// Load_info.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Load_info : Page
    {
        BackgroundWorker get_info_by_id;
        string gallery_num;
        private static readonly Regex numRegex = new Regex("^[0-9]+$"); //regex that matches disallowed text
        private static readonly Regex tagRegex = new Regex("^((female|male|artist|group|character|language|series|tag|type):[a-z_-]+( ){0,})+$"); //regex that matches disallowed text

        public Load_info()
        {
            InitializeComponent();
            get_info_by_id = new BackgroundWorker();
            get_info_by_id.DoWork += Get_info_by_id_DoWork;
            
        }

        private void search_Click(object sender, RoutedEventArgs e)
        {
            gallery_num = keyword.Text;
            search.IsEnabled = false;
            keyword.IsEnabled = false;
            get_info_by_id.RunWorkerAsync();
        }

       
        private void Get_info_by_id_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (numRegex.IsMatch(gallery_num))
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, () =>
                    {
                        Console.WriteLine("number match");
                        galleryInfo info = new galleryInfo(int.Parse(keyword.Text));
                        info.onViewClick += onViewClick;
                        loadResult.Children.Clear();
                        loadResult.Children.Add(info);
                        info.onLoaded += (sender, e) =>
                        {
                            Dispatcher.Invoke(() =>
                            {
                                search.IsEnabled = true;
                                keyword.IsEnabled = true;
                            });
                        };
                    });

                }
                else if (tagRegex.IsMatch(gallery_num))
                {
                    Console.WriteLine("tag match");
                    Dispatcher.Invoke(DispatcherPriority.Normal, () =>
                    {
                        searchResult info = new(utils.ParseTags(keyword.Text));
                        //info.onViewClick += onViewClick;
                        loadResult.Children.Clear();
                        loadResult.Children.Add(info);
                        info.onLoaded += (sender, e) =>
                        {
                            Dispatcher.Invoke(() =>
                            {
                                search.IsEnabled = true;
                                keyword.IsEnabled = true;
                            });
                        };
                    });
                }
                else
                {
                    MessageBox.Show("검색은 작품 번호 또는 태그로만 검색해주세요", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        search.IsEnabled = true;
                        keyword.IsEnabled = true;
                    }));
                }
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

        private void onViewClick(int page, Gallery gallery)
        {
            NavigationService.Navigate(new Viewer(gallery.files, page, gallery_num), UriKind.Relative);
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
                using (ZipArchive archive = ZipFile.OpenRead(diglog.FileName))
                {
                    foreach (ZipArchiveEntry zip in archive.Entries)
                    { 
                        if(new FileInfo(zip.FullName).Extension != ".png")
                        {
                            MessageBox.Show("파일을 불러오는중 오류가 발생했습니다.\n해당 압축 파일은 이미지 압축 모음이 아닌것 같습니다.", "", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }

                Viewer viewer = new Viewer(new string[] { diglog.FileName }, 1, null);
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

        private void keyword_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) search_Click(sender, null);
        }
    }
}
