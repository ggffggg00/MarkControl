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
using System.Windows.Shapes;
using System.IO;  

namespace WpfApp2.UI.Windows
{
    /// <summary>
    /// Логика взаимодействия для UserGuide.xaml
    /// </summary>
    /// 
    public partial class UserGuide : Window
    {

        bool isLight = false;
        public UserGuide()
        {
            InitializeComponent();
            br.Opacity = 0;

            loadpage();
        }
            
        private void WebBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            br.Opacity = 1;
        }

        void loadpage()
        {
            string applicationDirectory = Environment.CurrentDirectory;
            string myFile = System.IO.Path.Combine(applicationDirectory, isLight ? "manual/light.html" : "manual/index.html");
            br.Navigate(new Uri("file:///" + myFile));

            string colorCode = isLight ? "#ffffff" : "#363B40";

            this.Background = (SolidColorBrush) new BrushConverter().ConvertFrom(colorCode);

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            isLight = !isLight;
            loadpage();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!isLight)
                System.Diagnostics.Process.Start("https://markcontrol-85c25.firebaseapp.com/");
            else
                System.Diagnostics.Process.Start("https://markcontrol-85c25.firebaseapp.com/light.html");
            Close();
        }
    }
}
