
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfApp2.DB;
using WpfApp2.DB.Models;

namespace WpfApp2.UI.Components
{

    public partial class DataExplorerFragment : UserControl
    {
        DatabaseHelper db;
        ProjectData data;

        ObservableCollection<string> BlockList { get; set; }

        /// <summary>
        /// Экземпляр EventHandler для оповещения всех компонентов об изменении данных
        /// </summary>
        public event EventHandler<EventArgs> dataChangeEvent;

        public DataExplorerFragment(DatabaseHelper DB, ProjectData prData)
        {
            this.db = DB;
            this.data = prData;
            InitializeComponent();
            initBlocksList();
            showImage();
            setEmpty(data.epochCount == 0);

            if (data.epochCount != 0)
                initOrUpdateDataGrid();

        }

        void initOrUpdateDataGrid()
        {

            if (dtGrid.DataContext != null)
            {
                dtGrid.DataContext = null;
                dtGrid.ItemsSource = null;
            }

            DataTable Data = new DataTable();
            Data.Columns.Add("Эпоха");

            for (int i = 1; i <= data.marksCount; i++)
                Data.Columns.Add(i.ToString());

            foreach (MarksRow epoch in data.marks)
            {
                object[] values = new object[data.marksCount + 1];
                values[0] = epoch.epoch;

                foreach (KeyValuePair<int, double> markvalue in epoch.marks)
                    values[markvalue.Key] = markvalue.Value;

                Data.Rows.Add(values);


            }

            dtGrid.DataContext = Data.DefaultView;
                       


        }

        void notifyOnDataChanged() {

            setEmpty(data.epochCount == 0);

            if (data.epochCount != 0)
                initOrUpdateDataGrid();


            EventHandler<EventArgs> handler = dataChangeEvent;



            if (handler != null)
                handler(this, new EventArgs());
        }

        void initBlocksList() {

            this.BlockList = new ObservableCollection<string>();

            this.LV.DataContext = BlockList;
            this.LV.ItemsSource = BlockList;

            foreach (JObject blData in data.markInBlockOrder)
            {
                string res = "Блок "+(string)blData["blockName"]+": ";
                foreach (int mark in blData["marks"])
                    res += mark.ToString()+" ";

                BlockList.Add(res);

            }


        }



        void showImage() {

            byte[] buffer = data.img;
            ImageSource result;
            using (var stream = new MemoryStream(buffer))
            {
                result = BitmapFrame.Create(
                    stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }

            img.Source = result;
        }


        void setEmpty(bool isEmpty) {
            this.dtGrid.Visibility = isEmpty ? System.Windows.Visibility.Hidden : System.Windows.Visibility.Visible;
            this.emptyLabels.Visibility = !isEmpty ? System.Windows.Visibility.Hidden : System.Windows.Visibility.Visible;
        
        }


        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            notifyOnDataChanged();
        }


        //Импорт значений
        private void Button_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".csv";
            dlg.Filter = "Данные (.csv)|*.csv";
            dlg.Multiselect = false;

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true) {
                data.AddAllMarks(ImportExportManager.parseCsv(dlg.FileName));
                notifyOnDataChanged();
            }

        }
    }
}
