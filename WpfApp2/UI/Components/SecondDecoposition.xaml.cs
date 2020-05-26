
using Newtonsoft.Json.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp2.DB.Models;

namespace WpfApp2.UI.Components
{
    /// <summary>
    /// Логика взаимодействия для SecondDecoposition.xaml
    /// </summary>
    public partial class SecondDecoposition : UserControl, DataChangeNotifier
    {
        ProjectData data;

        public SecondDecoposition(ProjectData data)
        {
            this.data = data;
            InitializeComponent();
            fillCombobox();

        }

        void fillCombobox()
        {
            cmbb.Items.Clear();
            foreach (JObject obj in data.markInBlockOrder)
                cmbb.Items.Add("Блок " + (string)obj["blockName"]);
        }

        void showBlockDecomposition(string blockName) {

            FirstDecomposition fr = new FirstDecomposition(data, getBlockMarks(blockName));
            cc.Content = fr;
        
        }


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


        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            

            string lbi = (sender as ComboBox).SelectedItem as string;
            var blockName = lbi.Replace("Блок ","").Trim();
            showBlockDecomposition(blockName);

        }

        public void onDataChanged(DataChangedEventArgs e)
        {
            fillCombobox();
        }
    }
}
