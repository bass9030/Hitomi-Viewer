using cs_hitomi;
using System;
using System.Collections.Generic;
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
    /// <summary>
    /// galleryDetails.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class galleryDetails : UserControl
    {
        public galleryDetails(string[] artists, string[] groups, string[] serieses, string type, string language, Tag[] tags, DateTime publishedDate, bool isShort = false)
        {
            InitializeComponent();

            if(isShort)
            {
                //@long.Visibility = Visibility.Collapsed;
                //@short.Visibility = Visibility.Visible;
                /*List<string> _tags = new List<string>();
                foreach (Tag tag in tags) _tags.Add(tag.ToString());
                content.Text =  "종류: " + type + "\n" +
                    "작가: " + (artists.Length != 0 ? string.Join(", ", artists) : "N/A") + (groups.Length != 0 ? "(" + string.Join(", ", groups) + ")" : "") + "\n" +
                    ((serieses.Length != 0) ? "원작: " + string.Join(", ", serieses) + "\n" : "") +
                    "언어: " + language + "\n" +
                    "태그: " + string.Join(", ", _tags) + "\n" +
                    "업로드 날짜: " + publishedDate.ToString();*/

                artistPanel.Orientation = Orientation.Horizontal;
                artistLabel.FontSize = 15;
                seriesPanel.Orientation = Orientation.Horizontal;
                seriesLabel.FontSize = 15;
                typePanel.Orientation = Orientation.Horizontal;
                typeLabel.FontSize = 15;
                languagePanel.Orientation = Orientation.Horizontal;
                languageLabel.FontSize = 15;
                tagPanel.Orientation = Orientation.Horizontal;
                tagLabel.FontSize = 15;
                publishedDatePanel.Orientation = Orientation.Horizontal;
                publicLabel.FontSize = 15;
            }


            artist.Content = (artists.Length != 0 ? string.Join(", ", artists) + (groups.Length != 0 ? $"({string.Join(", ", groups)})" : "") : "N/A");

            if (serieses.Length != 0)
                series.Content = string.Join(", ", serieses);
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
    }
}
