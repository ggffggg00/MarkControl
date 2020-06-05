using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp2.DB.Models;
using WpfApp2.Utils;

namespace WpfApp2.UI.Components
{
    /// <summary>
    /// Логика взаимодействия для FourthDecomposition.xaml
    /// </summary>
    public partial class FourthDecomposition : UserControl, DataChangeNotifier
    {
        ProjectData data;

        double chartMin = 100500;
        double chartMax = 0;
        double scaleCoef = 0.1;

        public FourthDecomposition(ProjectData data)
        {
            this.data = data;
            InitializeComponent();
            initMarksList();
            showImage();
        }


        /// <summary>
        /// Загружает список блоков с чекбоксами для графика
        /// </summary>
        void initMarksList()
        {
            foreach (int mark in data.marks[0].marks.Keys.ToArray())
            {
                CheckBoxListViewItem item = new CheckBoxListViewItem(mark.ToString(), false);
                LV.Items.Add(item);
            }
                
        }

        /// <summary>
        /// Загружает и показывает схему объекта
        /// </summary>
        void showImage()
        {

            byte[] buffer = data.img;
            ImageSource result;
            using (var stream = new MemoryStream(buffer))
            {
                result = BitmapFrame.Create(
                    stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }

            img.Source = result;
        }

        /// <summary>
        /// Инициализирует или обновляет график актуальными данными
        /// </summary>
        private void showOrUpdateChart()
        {

            Chart chart = this.FindName("MyWinformChart") as Chart;

            chart.Series.Clear();

            ChartHelper.styleChart(chart);
            chart.ChartAreas[0].AxisX.Title = "Эпоха";
            chart.ChartAreas[0].AxisY.Title = "Высота";

            foreach (int mark in data.marks[0].marks.Keys)
            {

                Series ser = ChartHelper.constructSeries(mark.ToString(), chart);
                ser.XValueMember = "Эпоха";
                ser.YValueMembers = "Высота";

                foreach(int epoch in Enumerable.Range(0, data.epochCount))
                {
                    double markValue = data.marks[epoch].marks[mark];
                    DataPoint point = ChartHelper.constructDataPoint(epoch, markValue);
                    point.MarkerSize = 10;
                    //point.ToolTip = string.Format("Эпоха: {0}, Высота: {1}", epoch, markValue);
                    ser.Points.Add(point);

                    if (markValue > chartMax)
                        chartMax = markValue;
                    else if (markValue < chartMin)
                        chartMin = markValue;
                }

                ser.Enabled = ((CheckBoxListViewItem)LV.Items[mark-1]).IsChecked;

                chart.Series.Add(ser);

            }
            double scaleOffset = (chartMax - chartMin) * scaleCoef;
            chart.ChartAreas[0].AxisY.Maximum = chartMax + scaleOffset;
            chart.ChartAreas[0].AxisY.Minimum = chartMin - scaleOffset;

            chart.Refresh();

           


        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            showOrUpdateChart();
        }

        public void onDataChanged(DataChangedEventArgs e)
        {
            showOrUpdateChart();
        }
    }
}
