using cs_hitomi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
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
        bool isEventRegistered = false;


        public searchResult(Tag[] _tags)
        {
            tags = _tags;
            InitializeComponent();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerAsync();
        }

        private void Result_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            if (scrollViewer.VerticalOffset + 100 >= scrollViewer.ScrollableHeight)
            {
                page++;
                if(!worker.IsBusy) worker.RunWorkerAsync();
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            getIdsOptions option = new getIdsOptions()
            {
                startIndex = (page - 1) * 25,
                endIndex = (page - 1) * 25 + 24,
                tags = tags
            };
            int[] ids;
            while(true)
            {
                try
                {
                    ids = utils.getIds(option);
                    break;
                }
                catch { }
            }
            foreach(int id in ids)
            {

                Dispatcher.Invoke(() =>
                {
                    galleryInfo infoGallery = new(id, true);
                    infoGallery.Height = 350;
                    Thickness margin = new()
                    {
                        Left = 0,
                        Right = 0,
                    };
                    infoGallery.Margin = margin;
                    infoGallery.onViewClick += onViewClick;

                    Border hr = new Border();
                    hr.Height = 1;
                    hr.Background = Brushes.Gray;
                    ((StackPanel)result.Content).Children.Add(infoGallery);
                    ((StackPanel)result.Content).Children.Add(hr);
                });

            }
            if (!isEventRegistered)
            {
                result.ScrollChanged += Result_ScrollChanged;
                isEventRegistered = true;
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
