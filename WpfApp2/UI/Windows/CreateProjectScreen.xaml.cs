using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WpfApp2
{

    public struct ProjectInitialData
    {
        public string title;
        public string markCount;
        public string blockCount;
        public string imagePath;

    }
    

    public partial class CreateProjectScreen : Window
    {
        private DatabaseHelper DBHelper;
        public ProjectInitialData data;
        public long insertedId { get; set; } = 0;


        public CreateProjectScreen(DatabaseHelper DBHelper)
        {
            this.DBHelper = DBHelper;
            this.data = new ProjectInitialData();
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.data.title = TitleField.Text;
            this.data.markCount = MarkCountField.Text;
            this.data.blockCount = BlockCountField.Text;

            if (data.title == null || data.blockCount == null || data.markCount == null)
            {
                showAlert(@"Заполните все поля");
                return;
            }
            if (data.imagePath == null)
            {
                showAlert(@"Загрузите изображение, содердащее план объекта");
                return;
            }

            this.insertedId = new CreateProjectRequest(DBHelper, data).execute();
            this.DialogResult = true;
            new CreateProjectTableRequest(DBHelper, insertedId, Int32.Parse(data.markCount)).execute();
            this.Close();

        }

        private void showAlert(string msg)
        {
            MessageBox.Show(msg, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "Изображение (.png)|*.png";
            dlg.Multiselect = false;

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true) {
                this.data.imagePath = dlg.FileName;
                loadPreview(this.data.imagePath);
            }


            
        }

        private void loadPreview(string path)
        {
            BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            bi3.UriSource = new Uri(path, UriKind.Absolute);
            bi3.EndInit();
            ImagePreview.Source = bi3;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
