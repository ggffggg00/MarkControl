
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
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp2.Calc;
using WpfApp2.DB.Models;
using WpfApp2.UI.Windows;
using WpfApp2.Utils;

namespace WpfApp2.UI.Components
{

    class CheckBoxListViewItem
    {


        public bool IsChecked { get; set; }
        public string Text { get; set; }

        public CheckBoxListViewItem(string text, bool isChecked = true)
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

        double chartMin = 100500;
        double chartMax = 0;
        double scaleCoef = 0.1;

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

            chartMin = 100500;
            chartMax = 0;

            Chart chart = this.FindName("MyWinformChart") as Chart;

            chart.Series.Clear();

            ChartHelper.styleChart(chart);

            for (int x = 0; x < data.markInBlockOrder.Count; x++)
            {

                if (!((CheckBoxListViewItem)LV.Items[x]).IsChecked)
                    continue;

                JObject block = (JObject)data.markInBlockOrder[x];

                string blname = (string)block["blockName"];
                Series ser = ChartHelper.constructSeries(blname, chart);

                FirstDecompositionCalculator calc = new FirstDecompositionCalculator(data, getBlockMarks(blname));

                for (int i = 0; i < data.epochCount; i++)
                    ser.Points.Add(constructDataPoint(calc.calculateM(i), calc.calculateAlpha(i)));

                chart.Series.Add(ser);
                   
            }

            double scaleOffset = (chartMax - chartMin) * scaleCoef;

            chart.ChartAreas[0].AxisX.Maximum = chartMax + scaleOffset;
            chart.ChartAreas[0].AxisX.Minimum = chartMin - scaleOffset;

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
            showOrUpdateChart();
        }

        #region Настройка графика
        //++++++++++++++МЕТОДЫ КОНФИГУРАЦИИ ГРАФИКА++++++++++++++++

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

        #endregion

        const double MINIMIZED_HEIGHT = 150;        //Нормальная высота
        const double MAXIMIZED_HEIGHT = 350;        //Высота увеличенного графика
        const double ANIMATION_DURATION = 150;      //Длительность анимации в миллисекундах

        //Событие срабатывает при наведении курсора мыши на график в боковой панели
        private void MyWinformChart_MouseEnter(object sender, EventArgs e)
        {
            //Создаем объект анимации значения
            //Эта штука работает довольно просто, мы говорим ей от какого числа и до какого числа перебрать значения
            //Потом мы указываем, значения какого параметра и какого элемента мы перебираем
            //А потом просто запускаем анимацию
            DoubleAnimation buttonAnimation = new DoubleAnimation();                //Создаем объект анимации
            buttonAnimation.From = MINIMIZED_HEIGHT;                                //Указываем начальное значение, от которого будет начинаться перебор
            buttonAnimation.To = MAXIMIZED_HEIGHT;                                  //Указываем конечное значение, до которого будет происходить перебор
            buttonAnimation.Duration = TimeSpan.FromMilliseconds(ANIMATION_DURATION);   //Указываем время, за которое значения должны быть перебраны

            host.BeginAnimation(WindowsFormsHost.HeightProperty, buttonAnimation);      //Запускаем анимацию в контейнере графика, что надо записывать перебираемые значения в свойство высоты
        }

        private void MyWinformChart_MouseLeave(object sender, EventArgs e)
        {
            DoubleAnimation buttonAnimation = new DoubleAnimation();
            buttonAnimation.From = MAXIMIZED_HEIGHT;
            buttonAnimation.To = MINIMIZED_HEIGHT;
            buttonAnimation.Duration = TimeSpan.FromMilliseconds(ANIMATION_DURATION);
            host.BeginAnimation(WindowsFormsHost.HeightProperty, buttonAnimation);
        }

        private void img_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            new ImageViewer(data.img).Show();
        }
    }
}
