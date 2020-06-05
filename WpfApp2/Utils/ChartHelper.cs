using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace WpfApp2.Utils
{
    class ChartHelper
    {


        /// <summary>
        /// Настраивает график согласно нашему стилю (да, да, костыль, но другого выбора нет)
        /// </summary>
        public static void styleChart(Chart chart)
        {
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
        public static DataPoint constructDataPoint(double x, double y)
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
        public static Series constructSeries(string name, Chart chart)
        {

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
