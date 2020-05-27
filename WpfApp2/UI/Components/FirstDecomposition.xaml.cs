using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Controls;
using WpfApp2.Calc;
using WpfApp2.DB.Models;
using System.Windows;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms;
using System.Drawing;

namespace WpfApp2.UI.Components
{
    /// <summary>
    /// Логика взаимодействия для FirstDecomposition.xaml
    /// </summary>
    /// 
       


    public partial class FirstDecomposition : System.Windows.Controls.UserControl, DataChangeNotifier
    {
        ProjectData data;
        FirstDecompositionCalculator calc;
        bool chartLoaded = false;

        public FirstDecomposition(ProjectData data, int[] marksToCalculate = null)
        {
            this.data = data;
            this.calc = new FirstDecompositionCalculator(this.data, marksToCalculate);

            InitializeComponent();


            showOrUpdateChart();

            //showOrUpdateChart();
            buildTable();

        }

        private void showOrUpdateChart()
        {

            Chart chart = this.FindName("MyWinformChart") as Chart;

            Color fcolor = Color.FromArgb(185, 185, 185);

            chart.ChartAreas[0].AxisX.LineColor = fcolor;
            chart.ChartAreas[0].AxisX.LabelStyle.Format = "#.#####";
            chart.ChartAreas[0].AxisX.IsMarginVisible = false;
            chart.ChartAreas[0].AxisX.MajorGrid.LineColor = fcolor;
            chart.ChartAreas[0].AxisX.LabelStyle.ForeColor = fcolor;

            chart.ChartAreas[0].AxisY.LineColor = fcolor;
            chart.ChartAreas[0].AxisY.LabelStyle.Format = "#.#####";
            chart.ChartAreas[0].AxisY.IsMarginVisible = false;
            chart.ChartAreas[0].AxisY.MajorGrid.LineColor = fcolor;
            chart.ChartAreas[0].AxisY.LabelStyle.ForeColor = fcolor;

            //If you are looking to change the color of the Grid Lines
            chart.ChartAreas[0].AxisX.MajorGrid.LineColor = fcolor;
            chart.ChartAreas[0].AxisY.MajorGrid.LineColor = fcolor;

            if( !chartLoaded)
            {
                Series mSeries = constructSeries("Фазовая траектория");
                Series mPSeries = constructSeries("Нижний предел");
                Series mMSeries = constructSeries("Верхний предел");
                Series mPredictSeries = constructSeries("Прогнозируемая траектория");

                chart.Series.Add(mSeries);
                chart.Series.Add(mPSeries);
                chart.Series.Add(mMSeries);
                chart.Series.Add(mPredictSeries);
            } else
            {
                chart.Series[0].Points.Clear();
                chart.Series[1].Points.Clear();
                chart.Series[2].Points.Clear();
                chart.Series[3].Points.Clear();
            }
            

            for (int i = 0; i<data.epochCount; i++)
            {
                chart.Series[0].Points.Add(constructDataPoint(calc.calculateM(i), calc.calculateAlpha(i)));
                chart.Series[1].Points.Add(constructDataPoint(calc.calculateM_plus(i), calc.calculateAlpha(i)));
                chart.Series[2].Points.Add(constructDataPoint(calc.calculateM_minus(i), calc.calculateAlpha(i)));
                chart.Series[3].Points.Add(constructDataPoint(calc.calculateMPredict(i), calc.calculateAlpha(i)));
            }

            chartLoaded = true;

            chart.Update();


        }

        DataPoint constructDataPoint(double x, double y) {
            var dp = new DataPoint(x, y);
            dp.IsValueShownAsLabel = true;
            dp.MarkerSize = 8;
            dp.LabelForeColor = Color.FromArgb(185, 185, 185); 
            dp.ToolTip = string.Format("M: {0}, Alpha: {1}", x, y);
            dp.Label = "#INDEX";
            return dp;
        }

        Series constructSeries(string name) {
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
        
        void buildTable()
        {
            if(dtGrid.DataContext == null)
            {
                dtGrid.DataContext = null;
                dtGrid.ItemsSource = null;
            }

            DataTable Data = new DataTable();

            Data.Columns.Add("Эпоха");
            Data.Columns.Add("М");
            Data.Columns.Add("М pr");
            Data.Columns.Add("Alpha");
            Data.Columns.Add("M+"); 
            Data.Columns.Add("M-");
            Data.Columns.Add("Alpha+");
            Data.Columns.Add("Alpha-");
            Data.Columns.Add("Допуск");


            for (int index = 0; index < data.marks.Count; index++)
            { 
                object[] values = new object[9];
                var item = data.marks[index];


                double m = calc.calculateM(index);
                double alpha = calc.calculateAlpha(index);
                double mPlus = calc.calculateM_plus(index);
                double mMinus = calc.calculateM_minus(index);
                double mPredict = calc.calculateMPredict(index);

                values[0] = item.epoch;
                values[1] = String.Format("{0:0.######}", m);
                values[2] = String.Format("{0:0.######}", mPredict);
                values[3] = String.Format("{0:0.#########}", alpha);
                values[4] = String.Format("{0:0.######}", mPlus);
                values[5] = String.Format("{0:0.######}", mMinus);
                values[6] = String.Format("{0:0.#########}", calc.calculateAlpha_plus(index));
                values[7] = String.Format("{0:0.#########}", calc.calculateAlpha_minus(index));
                values[8] = calc.hasStable(index);
                
                Data.Rows.Add(values);
            }

            dtGrid.DataContext = Data.DefaultView;
            dtGrid.ItemsSource = Data.DefaultView;
        }

        /// <summary>
        /// Метод реализует логику работы при изменении данных проекта
        /// </summary>
        public void onDataChanged(DataChangedEventArgs e)
        {
            showOrUpdateChart();
            buildTable();
        }

        private void mCheckbox_Click(object sender, RoutedEventArgs e)
        {
            Chart chart = this.FindName("MyWinformChart") as Chart;
            chart.Series[0].Enabled = (bool)mCheckbox.IsChecked;
            chart.Series[1].Enabled = (bool)mPCheckbox.IsChecked;
            chart.Series[2].Enabled = (bool)mMCheckbox.IsChecked;
            chart.Series[3].Enabled = (bool)mMCheckbox.IsChecked;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
