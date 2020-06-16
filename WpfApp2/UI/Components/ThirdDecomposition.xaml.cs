using Newtonsoft.Json.Linq;
using Orbifold.Graphite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfApp2.Calc;
using WpfApp2.DB.Models;
using WpfApp2.UI.Windows;
using WpfApp2.Utils;

namespace WpfApp2.UI.Components
{
    /// <summary>
    /// Логика взаимодействия для ThirdDecomposition.xaml
    /// </summary>
    /// 

    public partial class ThirdDecomposition : UserControl, DataChangeNotifier
    {

        double chartMin = 100500;
        double chartMax = 0;
        double scaleCoef = 0.1;

        ProjectData data;
        NetCalculator calc;
        BlockObject parentBlock;

        List<BlockObject> subBlockList = new List<BlockObject>();
        DataChangeNotifier decompositionContentDelegate;

        public ThirdDecomposition(ProjectData data)
        {
            this.data = data;

            InitializeComponent();
            fillCombobox();

            //UpdateUI();
            getStarted(true);

        }


        void fillCombobox()
        {
            cmbb.Items.Clear();
            foreach (JObject obj in data.markInBlockOrder)
                cmbb.Items.Add(BlockObject.FromJson(obj).ToString());
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex(@"^[0-9]*(?:\,[0-9]*)?$");
            if (regex.IsMatch(e.Text) && !(e.Text == "," && ((TextBox)sender).Text.Contains(e.Text)))
                e.Handled = false;

            else
                e.Handled = true;
        }

        /// <summary>
        /// Загружает список блоков с чекбоксами для графика
        /// </summary>
        void initBlocksList()
        {
            LV.Items.Clear();
            foreach (BlockObject blData in subBlockList)
                LV.Items.Add(new CheckBoxListViewItem(blData.subblock().ToString()));
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

        void updateGraph()
        {
            diagram.NewDiagram(false);

            Brush purple = (SolidColorBrush)(new BrushConverter().ConvertFrom("#41416a"));

            diagram.nodeBackground = purple;
            diagram.nodeDefaultBorder = purple;

           

            var arrNodes = new Node[calc.CurrentBlockData.Count];

            for(int i = 0; i < calc.CurrentBlockData.Count; i++)
            {
                arrNodes[i] = new Node { Title = calc.CurrentBlockData.Marks[i].ToString() };
                diagram.AddNode(arrNodes[i]);
            }

            foreach(KeyValuePair<int,int> edge in calc.CurrentBlockData.EdgeIndexes)
            {
                diagram.LineBrush = calc.hasEdgeSolid(edge) ? Brushes.Green : Brushes.Red;
                diagram.AddEdge(arrNodes[edge.Key], arrNodes[edge.Value]);
            }

        }

        void showBlockDecomposition(int[] marks)
        {
            cc.Content = null;
            decompositionContentDelegate = null;
            FirstDecomposition fr = new FirstDecomposition(data, marks);
            decompositionContentDelegate = fr;
            cc.Content = fr;

        }

        public void onDataChanged(DataChangedEventArgs e)
        {
            updateUI();
            tbc.SelectedIndex = 0;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            int[] marksInBlock = parentBlock.marks;
            int blockCount;

            if (!Int32.TryParse(SubBlocksCountField.Text, out blockCount))
            {
                MessageBox.Show("Введите действительное число");
                return;
            }
                
            if (blockCount > marksInBlock.Length)
            {
                MessageBox.Show("Количество блоков не может быть больше, чем марок в блоке");
                return;
            }


            BlockInputFormDialog dlg = new BlockInputFormDialog(blockCount, marksInBlock, data.img, parentBlock.blockName);
            if (dlg.ShowDialog() != true)
                return;

            LV.UnselectAll();
            subBlockList.Clear();

            subBlockList.AddRange(dlg.Result);
            initBlocksList();
            showOrUpdateChart();

            DecompositionTab.Visibility = Visibility.Visible;

        }

        /// <summary>
        /// Инициализирует или обновляет график актуальными данными
        /// </summary>
        private void showOrUpdateChart()
        {
            chartMin = 100500;
            chartMax = 0;

            Chart chart = this.FindName("MyWinformChart") as Chart;

            chart.Series.Clear();

            ChartHelper.styleChart(chart);

            for (int x = 0; x < subBlockList.Count; x++)
            {

                if (!((CheckBoxListViewItem)LV.Items[x]).IsChecked)
                    continue;

                Series ser = ChartHelper.constructSeries(subBlockList[x].blockName, chart);

                FirstDecompositionCalculator calc = new FirstDecompositionCalculator(data, subBlockList[x].marks);

                for (int i = 0; i < data.epochCount; i++)
                {
                    DataPoint point = constructDataPoint(calc.calculateM(i), calc.calculateAlpha(i));
                    ser.Points.Add(point);
                }

                double scaleOffset = (chartMax - chartMin) * scaleCoef;

                chart.ChartAreas[0].AxisX.Maximum = chartMax + scaleOffset;
                chart.ChartAreas[0].AxisX.Minimum = chartMin - scaleOffset;

                chart.Series.Add(ser);

            }

            chart.Refresh();

        }

        /// <summary>
        /// Возвразает стилизованную точку графика
        /// </summary>
        /// <param name="x">Значение оси Х</param>
        /// <param name="y">Значение оси У</param>
        /// <returns>Точка данных для добавления в серию</returns>
        DataPoint constructDataPoint(double x, double y)
        {
            var dp = ChartHelper.constructDataPoint(x, y);
            if (x > chartMax)
                chartMax = x;
            if (x < chartMin)
                chartMin = x;

            return dp;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            showOrUpdateChart();
        }

        private void LV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LV.SelectedIndex == -1)
                return;

            showBlockDecomposition(subBlockList[LV.SelectedIndex].marks);
        }

        private void cmbb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            int selectedIndex = (sender as ComboBox).SelectedIndex;
            if (selectedIndex < 0)
                return;

            parentBlock = BlockObject.FromJson(data.markInBlockOrder[selectedIndex] as JObject);

            updateUI();
        }

        void getStarted(bool hasStart)
        {
            Visibility visReversed = hasStart ? Visibility.Hidden : Visibility.Visible;
            Visibility vis = !hasStart ? Visibility.Hidden : Visibility.Visible;

            tableContainer.Visibility = visReversed;
            chartContainer.Visibility = visReversed;
            GetStartedIndicator.Visibility = vis;
            sp1.Visibility = visReversed;
            sp2.Visibility = visReversed;
            sp3.Visibility = visReversed;
        }


        private void updateUI()
        {
            if (parentBlock == null)
                return;

            getStarted(false);
            calc = new NetCalculator(data, parentBlock);

            var table = calc.generateEdgeDifferencetable();

            dtGrid.ItemsSource = table.DefaultView;
            subBlockList.Clear();
            LV.Items.Clear();
            DecompositionTab.Visibility = Visibility.Hidden;
            showImage();
            updateGraph();
            showOrUpdateChart();

            NetCountIndicator.Content = string.Format("Достаточное количество связей: {0}", calc.EdgeCountForSolidBlock());
        }
    }
}
