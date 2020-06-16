using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfApp2.UI.Windows
{
    /// <summary>
    /// Логика взаимодействия для ChartWindow.xaml
    /// </summary>
    public partial class ImageViewer : Window
    {
        public ImageViewer(byte[] imageData)
        {
            InitializeComponent();

            byte[] buffer = imageData;
            ImageSource result;
            using (var stream = new MemoryStream(buffer))
            {
                result = BitmapFrame.Create(
                    stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }

            image.Source = result;
        }




    }
}
