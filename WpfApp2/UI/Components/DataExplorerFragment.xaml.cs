
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
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
        bool isUpToDate = true;

        ObservableCollection<string> BlockList { get; set; }

        /// <summary>
        /// Экземпляр EventHandler для оповещения всех компонентов об изменении данных
        /// </summary>
        public event EventHandler<DataChangedEventArgs> dataChangeEvent;

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

            eac.Text = String.Format("{0:0.#########}", data.eAccuracy);
            acoef.Text = String.Format("{0:0.#########}", data.eAccuracy);

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
            dtGrid.ItemsSource = Data.DefaultView;



        }

        void notifyOnDataChanged(bool isWriteDb = false) {

            setEmpty(data.epochCount == 0);

            if (data.epochCount != 0)
                initOrUpdateDataGrid();


            EventHandler<DataChangedEventArgs> handler = dataChangeEvent;

            this.isUpToDate = isWriteDb;

            if (handler != null)
                handler(this, new DataChangedEventArgs(isUpToDate, !isWriteDb));
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
            //notifyOnDataChanged();
        }
        
        //Импорт значений
        private void Button_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".csv";
            dlg.Filter = "Данные (.csv)|*.csv";
            dlg.Multiselect = false;

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true) {
                data.AddAllMarks(ImportExportManager.parseCsv(dlg.FileName, data.epochCount));
                notifyOnDataChanged();
            }

        }

        private void Button_Click_2(object sender, System.Windows.RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Значения марок (*.csv)|*.csv";
            saveFileDialog.CheckPathExists = true;
            saveFileDialog.Title = "Сохранение значений марок";
            saveFileDialog.CheckFileExists = true;
            if (saveFileDialog.ShowDialog() == true)
                ImportExportManager.exportCsv(data.marks, saveFileDialog.FileName);
        }

        private void Button_Click_3(object sender, System.Windows.RoutedEventArgs e)
        {
            var isSaved = data.SaveAllData(db);
            if ( isSaved )
                notifyOnDataChanged(true);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));
        }

        private void Button_Click_4(object sender, System.Windows.RoutedEventArgs e)
        {

            data.eAccuracy = Double.Parse(eac.Text.Trim(), CultureInfo.InvariantCulture);
            data.aAccuracy = Double.Parse(acoef.Text.Trim(), CultureInfo.InvariantCulture);

            notifyOnDataChanged();

        }
    }
}
