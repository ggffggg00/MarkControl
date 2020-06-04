using Newtonsoft.Json.Linq;
using Orbifold.Graphite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
using static WpfApp2.UI.Windows.BlockInputFormDialog;

namespace WpfApp2.UI.Components
{
    /// <summary>
    /// Логика взаимодействия для ThirdDecomposition.xaml
    /// </summary>
    /// 

    


    public partial class ThirdDecomposition : UserControl, DataChangeNotifier
    {
        ProjectData data;
        NetCalculator calc;
        string BlockName;
        List<BlockObject> blocksInfo = new List<BlockObject>();
        DataChangeNotifier decompositionContentDelegate;

        public ThirdDecomposition(ProjectData data, string BlockName)
        {
            this.data = data;

            InitializeComponent();

            if (BlockName == null)
                throw new ArgumentNullException("Имя блока не может отсутствовать");

            this.BlockName = BlockName;

            calc = new NetCalculator(data, BlockName);

            var table = calc.generateEdgeDifferencetable();

            dtGrid.ItemsSource = table.DefaultView;
            showImage();
            updateGraph();

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
        /// Возвращает массив марок по имени блока
        /// </summary>
        int[] getBlockMarks(string blockName)
        {

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
        /// Загружает список блоков с чекбоксами для графика
        /// </summary>
        void initBlocksList()
        {
            LV.Items.Clear();
            foreach (BlockObject blData in blocksInfo)
                LV.Items.Add(new CheckBoxListViewItem(blData.ToString()));
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
            //throw new NotImplementedException();
        }

        private void dtGrid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
          
            
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            int[] marksInBlock = getBlockMarks(BlockName);
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


            BlockInputFormDialog dlg = new BlockInputFormDialog(blockCount, marksInBlock, data.img);
            if (dlg.ShowDialog() != true)
                return;

            LV.UnselectAll();
            blocksInfo.Clear();

            blocksInfo.AddRange(dlg.Result);
            initBlocksList();
            showOrUpdateChart();

            DecompositionTab.Visibility = Visibility.Visible;

        }

        /// <summary>
        /// Инициализирует или обновляет график актуальными данными
        /// </summary>
        private void showOrUpdateChart()
        {

            Chart chart = this.FindName("MyWinformChart") as Chart;

            chart.Series.Clear();

            styleChart();

            for (int x = 0; x < blocksInfo.Count; x++)
            {

                Series ser = constructSeries(blocksInfo[x].blockName);

                FirstDecompositionCalculator calc = new FirstDecompositionCalculator(data, blocksInfo[x].marks);

                for (int i = 0; i < data.epochCount; i++)
                    ser.Points.Add(constructDataPoint(calc.calculateM(i), calc.calculateAlpha(i)));

                ser.Enabled = ((CheckBoxListViewItem)LV.Items[x]).IsChecked;

                chart.Series.Add(ser);

            }

            chart.Refresh();

        }

        //++++++++++++++МЕТОДЫ КОНФИГУРАЦИИ ГРАФИКА++++++++++++++++

        /// <summary>
        /// Настраивает график согласно нашему стилю (да, да, костыль, но другого выбора нет)
        /// </summary>
        void styleChart()
        {
            Chart chart = this.FindName("MyWinformChart") as Chart;

            System.Drawing.Color fcolor = System.Drawing.Color.FromArgb(185, 185, 185);

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
            dp.LabelForeColor = System.Drawing.Color.FromArgb(185, 185, 185);
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


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            for (int x = 0; x < LV.Items.Count; x++)
            {
                var du = (CheckBoxListViewItem)LV.Items[x];
                Chart chart = this.FindName("MyWinformChart") as Chart;
                chart.Series[x].Enabled = du.IsChecked;
            }
        }

        private void LV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LV.SelectedIndex == -1)
                return;

            showBlockDecomposition(blocksInfo[LV.SelectedIndex].marks);
        }
    }
}
