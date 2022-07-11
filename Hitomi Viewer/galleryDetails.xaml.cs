using cs_hitomi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

namespace Hitomi_Viewer
{
    public class MultiWidthConverter : IMultiValueConverter
    {

        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return System.Convert.ToDouble(value[0]) - System.Convert.ToDouble(value[1]);
            /*double width = System.Convert.ToDouble(value[0]) - System.Convert.ToDouble(value[1]);
            return (width <= 0) ? double.NaN : width;*/
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// galleryDetails.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class galleryDetails : UserControl
    {
        public galleryDetails(string[] artists, string[] groups, string[] serieses, string type, string language, Tag[] tags, DateTime publishedDate, bool isShort = false)
        {
            InitializeComponent();
            DataContext = this;

            SizeChanged += GalleryDetails_SizeChanged;


            artist.Text = (artists.Length != 0 ? string.Join(", ", artists) + (groups.Length != 0 ? $"({string.Join(", ", groups)})" : "") : "N/A");

            if (serieses.Length != 0 && !serieses.Contains("original"))
                series.Text = string.Join(", ", serieses);
            else
                seriesPanel.Visibility = Visibility.Collapsed;

            this.type.Content = type;

            this.language.Content = language;

            foreach (Tag tag in tags)
            {
                tag tagControl = new tag(tag.type, tag.name);
                tagControl.VerticalAlignment = VerticalAlignment.Top;
                tagControl.HorizontalAlignment = HorizontalAlignment.Right;
                this.tag.Children.Add(tagControl);
            }

            this.publishedDate.Content = publishedDate.ToString();

        }

        private void GalleryDetails_SizeChanged(object sender, SizeChangedEventArgs e)
        { 
            /*Debug.WriteLine("{0}", Convert.ToString(tagPanel.ActualWidth - tagLabel.ActualWidth));

            Dispatcher.Invoke(() => tag.MaxWidth = tagPanel.ActualWidth - tagLabel.ActualWidth);*/
        }
    }
}
