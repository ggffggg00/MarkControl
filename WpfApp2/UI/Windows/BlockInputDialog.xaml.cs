using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace WpfApp2.UI.Windows
{
    /// <summary>
    /// Логика взаимодействия для BlockInputDialog.xaml
    /// </summary>
    public partial class BlockInputDialog : Window
    {

        int marksCount;
        int totalMarks;
        string blockName;
        public int[] marks;

        public BlockInputDialog(int marksCount, int totalMarks, string blockName)
        {
            this.marksCount = marksCount;
            this.blockName = blockName;
            this.totalMarks = totalMarks;
            InitializeComponent();
            label.Text = "Укажите марки для блока " + blockName;
        }

        public string ResponseText
        {
            get { return ResponseTextBox.Text; }
            set { ResponseTextBox.Text = value; }
        }

        private void showAlert(string msg)
        {
            MessageBox.Show(msg, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ResponseText.Length == 0)
            {
                showAlert("Поле не может быть пустым");
                return;
            }

            string[] values = ResponseText.Split(',');

            if (values.Length != marksCount)
            {
                showAlert("В поле должно быть " + marksCount.ToString() + " марки(ок)");
                return;
            }

            this.marks = new int[marksCount];

            try
            {
                for (int i = 0; i < values.Length; i++)
                {
                    int temp = Int32.Parse(values[i].Trim());
                    if (temp > totalMarks)
                        throw new ArgumentException("Недопустимый номер марки в позиции" + (i + 1).ToString());
                    this.marks[i] = temp;
                }

            }
            catch (ArgumentException e1)
            {
                showAlert(e1.Message);
                return;
            }
            catch (Exception e2)
            {
                showAlert("Неверный формат введенных данных");
                return;
            }

            DialogResult = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
