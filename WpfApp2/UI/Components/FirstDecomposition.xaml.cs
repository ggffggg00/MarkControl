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

namespace WpfApp2.UI.Components
{
    /// <summary>
    /// Логика взаимодействия для FirstDecomposition.xaml
    /// </summary>
    public partial class FirstDecomposition : UserControl, DataChangeNotifier
    {
        ProjectData data;
        FirstDecompositionCalculator calc;

        public FirstDecomposition(ProjectData data)
        {
            this.data = data;
            this.calc = new FirstDecompositionCalculator(this.data);
            InitializeComponent();
            initDataGrid();

        }

        
        void initDataGrid()
        {
            DataTable Data = new DataTable();

            Data.Columns.Add("Эпоха");
            Data.Columns.Add("М");
            Data.Columns.Add("Alpha");
            Data.Columns.Add("M+"); 
            Data.Columns.Add("M-");
            Data.Columns.Add("Alpha+");
            Data.Columns.Add("Alpha-");
            Data.Columns.Add("Устойчивость Т");
            Data.Columns.Add("Устойчивость П");
            Data.Columns.Add("Допуск");


            for (int index = 0; index < data.marks.Count; index++)
            { 
                object[] values = new object[10];
                var item = data.marks[index];

                System.Console.Out.WriteLine(calc.calculateAlpha_plus(index));
                System.Console.Out.WriteLine(calc.calculateAlpha_minus(index));

                values[0] = item.epoch;
                values[1] = String.Format("{0:0.######}", calc.calculateM(index));
                values[2] = String.Format("{0:0.#########}", calc.calculateAlpha(index));
                values[3] = String.Format("{0:0.######}", calc.calculateM_plus(index));
                values[4] = String.Format("{0:0.######}", calc.calculateM_minus(index));
                values[5] = String.Format("{0:0.#########}", calc.calculateAlpha_plus(index));
                values[6] = String.Format("{0:0.#########}", calc.calculateAlpha_minus(index));
                values[7] = String.Format("{0:0.########}", calc.stabilityTheory(index));
                values[8] = String.Format("{0:0.########}", calc.stabilityPractice(index));
                values[9] = calc.hasStable(index);
                

                Data.Rows.Add(values);
            }

            dtGrid.DataContext = Data.DefaultView;
        }

        /// <summary>
        /// Метод реализует логику работы при изменении данных проекта
        /// </summary>
        public void onDataChanged(EventArgs e)
        {
            throw new NotImplementedException();
        }


    }
}
