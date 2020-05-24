using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Imaging;
using WpfApp2.UI.Windows;

namespace WpfApp2
{

    public struct ProjectInitialData
    {
        public string title;
        public string markCount;
        public string blockCount;
        public string imagePath;
        public JObject metaData;

    }
    

    public partial class CreateProjectScreen : Window
    {
        private DatabaseHelper DBHelper;
        public ProjectInitialData data;
        bool fieldsEmpty = true;

        ObservableCollection<string> BlockList { get; set; }

        public long insertedId { get; set; } = 0;


        public CreateProjectScreen(DatabaseHelper DBHelper)
        {
            this.DBHelper = DBHelper;
            this.data = new ProjectInitialData();
            InitializeComponent();
            this.data.metaData = new JObject();
            this.data.metaData["blockData"] = new JObject();
            this.BlockList = new ObservableCollection<string>();
            this.LV.DataContext = BlockList;
            this.LV.ItemsSource = BlockList;
            this.MSP.Visibility = Visibility.Collapsed;
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            

            if (fieldsEmpty)
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


        //Кнопка выбора изображения
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "Изображение (.png)|*.png";
            dlg.Multiselect = false;

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                this.data.imagePath = dlg.FileName;
                loadPreview(this.data.imagePath);
                SortMarksInBlocks();
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


        //кнопка отмены
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void SortMarksInBlocks()
        {
            if (fieldsEmpty)
            {
                showAlert(@"Заполните все поля");
                return;
            }

            int bc = Int32.Parse(data.blockCount);
            int mc = Int32.Parse(data.markCount);          

            int marksPerBlock = mc / bc;

            JArray arr = new JArray();

            for (int i = 1; i <= bc; i++)
            {
                string blockName = ((char)(64 + i)).ToString();
                var dialog = new BlockInputDialog(marksPerBlock, mc,blockName);
                if (dialog.ShowDialog() == true)
                {
                    var blockObj = new JObject();
                    blockObj.Add("blockName", blockName);
                    blockObj.Add("marks", JArray.FromObject(dialog.marks));
                    arr.Add(blockObj);

                    string ints = string.Join(", ", dialog.marks);

                    BlockList.Add("Блок " + blockName + ": " + ints);
                    this.MSP.Visibility = Visibility.Visible;
                    this.Width = 768;

                } else {
                    MessageBox.Show("Вы отменили распределение марок по блокам");
                    this.BlockList.Clear();
                    return;
                }
            }
            this.data.metaData["blockData"] = arr;

        }

        private void TitleField_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                this.data.title = TitleField.Text;
                this.data.markCount = MarkCountField.Text;
                this.data.blockCount = BlockCountField.Text;
                this.fieldsEmpty = false;
            } catch (NullReferenceException ex) {
                this.fieldsEmpty = true;
            }
        }
    }
}
