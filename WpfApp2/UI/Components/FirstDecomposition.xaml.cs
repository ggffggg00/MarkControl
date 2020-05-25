using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp2.Calc;
using WpfApp2.DB.Models;
using System.Windows.Forms.DataVisualization.Charting;

namespace WpfApp2.UI.Components
{
    /// <summary>
    /// Логика взаимодействия для FirstDecomposition.xaml
    /// </summary>
    /// 

        class ChartData  {

            public struct ChartEntry
        {
            public ChartEntry(double alpha, double m, int epoch)
            {
                this.alpha = alpha;
                this.m = m;
                this.epoch = epoch;
            }

            public double alpha { get; set; }
            public double m { get; set; }
            public int epoch { get; set; }
        }

        public ChartData()
        {
            this.mList = new List<ChartEntry>();
            this.mPlusList = new List<ChartEntry>();
            this.mMinusList = new List<ChartEntry>();
            this.mPredictList = new List<ChartEntry>();
        }

        public List<ChartEntry> mList { get; set; }
        public List<ChartEntry> mPlusList { get; set; } = new List<ChartEntry>();
        public List<ChartEntry> mMinusList { get; set; } = new List<ChartEntry>();
        public List<ChartEntry> mPredictList { get; set; } = new List<ChartEntry>();

        }

    public partial class FirstDecomposition : UserControl, DataChangeNotifier
    {
        ProjectData data;
        FirstDecompositionCalculator calc;
        ChartData chartData;


        public FirstDecomposition(ProjectData data)
        {
            this.data = data;
            this.calc = new FirstDecompositionCalculator(this.data);

            InitializeComponent();

            showOrUpdateChart();
            buildTable();
            

        }


        private void showOrUpdateChart()
        {
            lineChart.DataContext = null;
            this.chartData = new ChartData();
            lineChart.DataContext = this.chartData;
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

                this.chartData.mList.Add(new ChartData.ChartEntry(alpha,m, item.epoch));
                this.chartData.mPlusList.Add(new ChartData.ChartEntry(alpha, mPlus, item.epoch));
                this.chartData.mMinusList.Add(new ChartData.ChartEntry(alpha, mMinus, item.epoch));
                this.chartData.mPredictList.Add(new ChartData.ChartEntry(alpha, mPredict, item.epoch));


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

    }
}
