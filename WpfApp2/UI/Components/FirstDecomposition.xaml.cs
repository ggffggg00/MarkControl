﻿using System;
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
       

    struct TableRow  {
        public int epoch { get; }
        public double m{ get; }
        public double alpha{ get; }
        public double mPlus{ get; }
        public double mMinus{ get; }
        public double mPredict{ get; }
        public double alphaPlus{ get; }
        public double alphaMinus{ get; }
        public bool stability{ get; }
        public bool hasPredict { get; }

        public TableRow(int epoch, double m, double alpha, double mPlus, double mMinus, double mPredict, double alphaPlus, double alphaMinus, bool stability, bool hasPredict = false)
        {
            this.epoch = epoch;
            this.m = m;
            this.alpha = alpha;
            this.mPlus = mPlus;
            this.mMinus = mMinus;
            this.mPredict = mPredict;
            this.alphaPlus = alphaPlus;
            this.alphaMinus = alphaMinus;
            this.stability = stability;
            this.hasPredict = hasPredict;
        }
    }


    public partial class FirstDecomposition : System.Windows.Controls.UserControl, DataChangeNotifier
    {
        ProjectData data;
        FirstDecompositionCalculator calc;
        bool chartLoaded = false;

        double chartMin = 100500;
        double chartMax = 0;
        double scaleCoef = 0.1;


        public FirstDecomposition(ProjectData data, int[] marksToCalculate = null)
        {
            this.data = data;
            this.calc = new FirstDecompositionCalculator(this.data, marksToCalculate);

            InitializeComponent();
            showOrUpdateChart();
            buildTable();

        }

        private void showOrUpdateChart()
        {
            chartMin = 100500;
            chartMax = 0;

            Chart chart = this.FindName("MyWinformChart") as Chart;

            chart.Series.Clear();

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

                Series mSeries = constructSeries("Фазовая траектория");
                Series mPSeries = constructSeries("Нижний предел");
                Series mMSeries = constructSeries("Верхний предел");
                Series mPredictSeries = constructSeries("Прогнозируемая траектория");

            

                chart.Series.Add(mSeries);
                chart.Series.Add(mPSeries);
                chart.Series.Add(mMSeries);
                chart.Series.Add(mPredictSeries);



            for (int i = 0; i<data.epochCount; i++)
            {
                    chart.Series[0].Points.Add(constructDataPoint(calc.calculateM(i), calc.calculateAlpha(i)));
                    chart.Series[1].Points.Add(constructDataPoint(calc.calculateM_plus(i), calc.calculateAlpha(i)));
                    chart.Series[2].Points.Add(constructDataPoint(calc.calculateM_minus(i), calc.calculateAlpha(i)));
                    chart.Series[3].Points.Add(constructDataPoint(calc.calculateMPredict(i), calc.calculateAlpha(i)));
                
            }

            chart.Refresh();
            mCheckbox_Click(null, null);

            double scaleOffset = (chartMax - chartMin) * scaleCoef;

            chart.ChartAreas[0].AxisX.Maximum = chartMax + scaleOffset;
            chart.ChartAreas[0].AxisX.Minimum = chartMin - scaleOffset;


        }

        DataPoint constructDataPoint(double x, double y) {
            var dp = new DataPoint(x, y);
            dp.IsValueShownAsLabel = true;
            dp.MarkerSize = 8;
            dp.LabelForeColor = Color.FromArgb(185, 185, 185); 
            dp.ToolTip = string.Format("M: {0}, Alpha: {1}", x, y);
            dp.Label = "#INDEX";

            if (x > chartMax)
                chartMax = x;
            else if (x < chartMin)
                chartMin = x;

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
            dtGrid.ItemsSource = null;
            dtGrid.Items.Clear();

            bool hasStable = true;

            for (int index = 0; index < data.epochCount; index++)
            {

                
                var item = data.marks[index];

                
                TableRow row = new TableRow(
                    item.epoch,
                    calc.calculateM(index),
                    calc.calculateAlpha(index),
                    calc.calculateM_plus(index),
                    calc.calculateM_minus(index),
                    calc.calculateMPredict(index),
                    calc.calculateAlpha_plus(index),
                    calc.calculateAlpha_minus(index),
                    calc.hasStable(index)
                    );

                if (!row.stability)
                    hasStable = false;


                dtGrid.Items.Add(row);


            }

            updateLabel(hasStable);

            dtGrid.Items.Add(new TableRow(
                    data.epochCount,
                    0,
                    0,
                    0,
                    0,
                    calc.calculateMPredict(data.epochCount),
                    0,
                    0,
                    true,
                    true
                    ));

            




        }

        void updateLabel(bool hasStable)
        {
            statusLabel.Content = hasStable ?
                "Объект устойчив" :
                "Объект не устойчив";
            statusLabel.Foreground = hasStable ?
                System.Windows.Media.Brushes.Green :
                System.Windows.Media.Brushes.Red;

            statusDescription.Text = hasStable ?
                "Объект устойчив на всех эпохах. В проведении второго уровня декомпозиции нет необходимости" :
                "Объект не устойчив на некоторых эпохах. Рекомендуется исследовать объект на втором уровне декомпозиции";


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
            chart.Series[3].Enabled = (bool)mPredictCheckbox.IsChecked;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
