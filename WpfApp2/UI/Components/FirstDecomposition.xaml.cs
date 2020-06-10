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
using WpfApp2.Utils;

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
        public double alphaPredict{ get; }
        public double alphaPlus{ get; }
        public double alphaMinus{ get; }
        public bool stability{ get; }
        public bool hasPredict { get; }

        public TableRow(int epoch, double m, double alpha, double mPlus, double mMinus, double mPredict, double alphaPredict, double alphaPlus, double alphaMinus, bool stability, bool hasPredict = false)
        {
            this.epoch = epoch;
            this.m = m;
            this.alpha = alpha;
            this.mPlus = mPlus;
            this.mMinus = mMinus;
            this.mPredict = mPredict;
            this.alphaPredict = alphaPredict;
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

            ChartHelper.styleChart(chart);

            Series mSeries = ChartHelper.constructSeries("Фазовая траектория", chart);
            Series mPSeries = ChartHelper.constructSeries("Нижний предел", chart);
            Series mMSeries = ChartHelper.constructSeries("Верхний предел", chart);
            Series mPredictSeries = ChartHelper.constructSeries("Прогнозируемая траектория", chart);

            for (int i = 0; i<data.epochCount; i++)
            {
                if(mCheckbox.IsChecked == true)
                    mSeries.Points.Add(constructDataPoint(calc.calculateM(i), calc.calculateAlpha(i)));
                if (mPCheckbox.IsChecked == true)
                    mPSeries.Points.Add(constructDataPoint(calc.calculateM_plus(i), calc.calculateAlpha_plus(i)));
                if (mMCheckbox.IsChecked == true)
                    mMSeries.Points.Add(constructDataPoint(calc.calculateM_minus(i), calc.calculateAlpha_minus(i)));
                if (mPredictCheckbox.IsChecked == true)
                    mPredictSeries.Points.Add(constructDataPoint(calc.calculateMPredict(i), calc.calculateAlphaPredict(i)));
                
            }

            if (mCheckbox.IsChecked == true)
                chart.Series.Add(mSeries);
            if (mPCheckbox.IsChecked == true)
                chart.Series.Add(mPSeries);
            if (mMCheckbox.IsChecked == true)
                chart.Series.Add(mMSeries);
            if (mPredictCheckbox.IsChecked == true)
                chart.Series.Add(mPredictSeries);

            double scaleOffset = (chartMax - chartMin) * scaleCoef;

            chart.ChartAreas[0].AxisX.Maximum = chartMax + scaleOffset;
            chart.ChartAreas[0].AxisX.Minimum = chartMin - scaleOffset;

            chart.Refresh();


        }

        DataPoint constructDataPoint(double x, double y) {
            var dp = ChartHelper.constructDataPoint(x, y);
            if (x > chartMax)
                chartMax = x;
            if (x < chartMin)
                chartMin = x;

            return dp;
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
                    calc.calculateAlphaPredict(index),
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
                    calc.calculateAlphaPredict(data.epochCount),
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
                "Объект устойчив на всех эпохах. В проведении следующего уровня декомпозиции нет необходимости" :
                "Объект не устойчив на некоторых эпохах. Рекомендуется исследовать объект на следующем уровне декомпозиции";


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
            showOrUpdateChart();
        }

    }
}
