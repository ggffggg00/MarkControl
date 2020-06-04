
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp2.Calc;
using WpfApp2.DB.Models;

namespace WpfApp2.UI.Components
{

    class CheckBoxListViewItem
    {


        public bool IsChecked { get; set; }
        public string Text { get; set; }

        public CheckBoxListViewItem(string text,bool isChecked = true)
        {
            IsChecked = isChecked;
            Text = text;
        }
    }

    /// <summary>
    /// Логика взаимодействия для SecondDecoposition.xaml
    /// </summary>
    /// 
    public partial class SecondDecoposition : UserControl, DataChangeNotifier
    {
        ProjectData data;

        /// <summary>
        /// Содержит делегат метода контента декомпозиции для обовещения об обновлении данных
        /// </summary>
        DataChangeNotifier decompositionContentDelegate;

        public SecondDecoposition(ProjectData data)
        {
            this.data = data;
            InitializeComponent();

            fillCombobox();             //Заполняем комбобокс блоками
            initBlocksList();           //Заполняем список блоков для графика
            showOrUpdateChart();        //Инициализируем график
            showImage();                //Показываем схему объекта

        }

        /// <summary>
        /// Инициализирует или обновляет график актуальными данными
        /// </summary>
        private void showOrUpdateChart()
        {

            Chart chart = this.FindName("MyWinformChart") as Chart;

            chart.Series.Clear();

            styleChart();

            for (int x = 0; x < data.markInBlockOrder.Count; x++)
            {

                JObject block = (JObject)data.markInBlockOrder[x];


                string blname = (string)block["blockName"];
                Series ser = constructSeries(blname);

                FirstDecompositionCalculator calc = new FirstDecompositionCalculator(data, getBlockMarks(blname));

                for (int i = 0; i < data.epochCount; i++)
                    ser.Points.Add(constructDataPoint(calc.calculateM(i), calc.calculateAlpha(i)));

                ser.Enabled = ((CheckBoxListViewItem)LV.Items[x]).IsChecked;

                chart.Series.Add(ser);
                   
            }

            chart.Refresh();

        }
       
        /// <summary>
        /// Заполняет список блоков в комбобокс
        /// </summary>
        void fillCombobox()
        {
            cmbb.Items.Clear();
            foreach (JObject obj in data.markInBlockOrder)
                cmbb.Items.Add("Блок " + (string)obj["blockName"]);
        }

        /// <summary>
        /// Отображает контент декомпозиции блока
        /// </summary>
        /// <param name="blockName"> Имя блока </param>
        void showBlockDecomposition(string blockName) {
            cc.Content = null;
            decompositionContentDelegate = null;
            FirstDecomposition fr = new FirstDecomposition(data, getBlockMarks(blockName));
            decompositionContentDelegate = fr;
            cc.Content = fr;
        
        }

       /// <summary>
       /// Загружает список блоков с чекбоксами для графика
       /// </summary>
        void initBlocksList()
        {

            foreach (JObject blData in data.markInBlockOrder)
            {
                string res = "Блок " + (string)blData["blockName"] + ": ";
                foreach (int mark in blData["marks"])
                    res += mark.ToString() + ", ";

                LV.Items.Add(new CheckBoxListViewItem(res.Substring(0,res.Length-2)));

            }


        }

        /// <summary>
        /// Загружает и показывает схему объекта
        /// </summary>
        void showImage()
        {

            byte[] buffer = data.img;
            System.Windows.Media.ImageSource result;
            using (var stream = new MemoryStream(buffer))
            {
                result = BitmapFrame.Create(
                    stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }

            img.Source = result;
        }

        /// <summary>
        /// Возвращает массив марок по имени блока
        /// </summary>
        int[] getBlockMarks(string blockName) {

            foreach (JObject obj in data.markInBlockOrder)
            {
                var currName = ((string)obj["blockName"]);

                if (currName == blockName)
                {
                    var marksArray = (JArray)obj["marks"];
                    return marksArray.Select(jv => (int)jv).ToArray();
                }
            }

            return null;
        }

        /// <summary>
        /// Отрабатывапет выбор блока для отображения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            

            string lbi = (sender as ComboBox).SelectedItem as string;
            if (lbi == null)
                return;

            var blockName = lbi.Replace("Блок ","").Trim();
            showBlockDecomposition(blockName);

        }

        /// <summary>
        /// Вызывается при обновлении данных в обозревателе
        /// </summary>
        /// <param name="e"></param>
        public void onDataChanged(DataChangedEventArgs e)
        {
            if (decompositionContentDelegate != null)
                decompositionContentDelegate.onDataChanged(e);

            showOrUpdateChart();

        }

        /// <summary>
        /// Отрабатывает нажатие на чекбоксы в списке блоков
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            for (int x = 0; x < LV.Items.Count; x++)
            {
                var du = (CheckBoxListViewItem)LV.Items[x];
                Chart chart = this.FindName("MyWinformChart") as Chart;
                chart.Series[x].Enabled = du.IsChecked;


            }
        }

        //++++++++++++++МЕТОДЫ КОНФИГУРАЦИИ ГРАФИКА++++++++++++++++

        /// <summary>
        /// Настраивает график согласно нашему стилю (да, да, костыль, но другого выбора нет)
        /// </summary>
        void styleChart() {
            Chart chart = this.FindName("MyWinformChart") as Chart;

            Color fcolor = Color.FromArgb(185, 185, 185);

            chart.ChartAreas[0].AxisX.LineColor = fcolor;
            chart.ChartAreas[0].AxisX.Title = "M";
            chart.ChartAreas[0].AxisX.TitleForeColor = fcolor;
            chart.ChartAreas[0].AxisX.LabelStyle.Format = "#.#####";
            chart.ChartAreas[0].AxisX.IsMarginVisible = false;
            chart.ChartAreas[0].AxisX.MajorGrid.LineColor = fcolor;
            chart.ChartAreas[0].AxisX.LabelStyle.ForeColor = fcolor;

            chart.ChartAreas[0].AxisY.LineColor = fcolor;
            chart.ChartAreas[0].AxisY.Title = "Alpha''";
            chart.ChartAreas[0].AxisY.TitleForeColor = fcolor;
            chart.ChartAreas[0].AxisY.LabelStyle.Format = "#.#####";
            chart.ChartAreas[0].AxisY.IsMarginVisible = false;
            chart.ChartAreas[0].AxisY.MajorGrid.LineColor = fcolor;
            chart.ChartAreas[0].AxisY.LabelStyle.ForeColor = fcolor;

        }

        /// <summary>
        /// Возвразает стилизованную точку графика
        /// </summary>
        /// <param name="x">Значение оси Х</param>
        /// <param name="y">Значение оси У</param>
        /// <returns>Точка данных для добавления в серию</returns>
        DataPoint constructDataPoint(double x, double y)
        {
            var dp = new DataPoint(x, y);
            dp.IsValueShownAsLabel = true;
            dp.MarkerSize = 5;
            dp.LabelForeColor = Color.FromArgb(185, 185, 185);
            dp.ToolTip = string.Format("M: {0}, Alpha: {1}", x, y);
            dp.Label = "#INDEX";

            return dp;
        }

        /// <summary>
        /// Создает стилизованный и настроенный объект серии
        /// </summary>
        /// <param name="name">Название серии</param>
        /// <returns>Экземпляр серии для добавления в график</returns>
        Series constructSeries(string name)
        {
            Chart chart = this.FindName("MyWinformChart") as Chart;

            Series ser = new Series(name);
            ser.ChartType = SeriesChartType.Spline;
            ser.ChartArea = chart.ChartAreas[0].Name;
            ser.Legend = "legend1";
            ser.XValueMember = "М";
            ser.YValueMembers = "Alpha";

            ser.MarkerStyle = MarkerStyle.Circle;

            return ser;
        }


    }
}
