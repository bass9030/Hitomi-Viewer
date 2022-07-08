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
    /// tag.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class tag : UserControl
    {
        public tag(tagType type, string name)
        {
            InitializeComponent();
            BrushConverter bc = new();
            tagName.Content = $"{type}:{name}";
            switch(type)
            {
                case tagType.female:
                    tagBackground.Background = (Brush)bc.ConvertFrom("#f07878");
                    break;

                case tagType.male:
                    tagBackground.Background = (Brush)bc.ConvertFrom("#7c78f0");
                    break;

                default:
                    break;
            }
        }
    }
}
