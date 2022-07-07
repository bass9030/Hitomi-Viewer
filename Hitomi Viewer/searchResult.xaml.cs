using cs_hitomi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Hitomi_Viewer
{

    /// <summary>
    /// searchResult.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class searchResult : UserControl
    {
        BackgroundWorker worker = new();
        int page = 1;
        Tag[] tags;
        public EventHandler onLoaded;


        public searchResult(Tag[] _tags)
        {
            tags = _tags;
            InitializeComponent();
            worker.DoWork += Worker_DoWork;
            Loaded += SearchResult_Loaded;
        }

        private void SearchResult_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            worker.RunWorkerAsync();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            getIdsOptions option = new getIdsOptions()
            {
                startIndex = (page - 1) * 25,
                endIndex = (page - 1) * 25 + 24,
                tags = tags
            };
            int[] ids = utils.getIds(option);
            foreach(int id in ids)
            {

                Dispatcher.Invoke(() =>
                {
                    galleryInfo infoGallery = new(id);
                    infoGallery.Width = result.ActualWidth - 20;
                    infoGallery.Height = 350;
                    infoGallery.onViewClick += onViewClick;
                    ((StackPanel)result.Content).Children.Add(infoGallery);
                });

            }
            onLoaded?.Invoke(this, EventArgs.Empty);

        }
        private void onViewClick(int page, Gallery gallery)
        {
            NavigationService navigationService = NavigationService.GetNavigationService(this);
            navigationService.Navigate(new Viewer(gallery.files, page, gallery.id.ToString()), UriKind.Relative);
        }

    }
}
