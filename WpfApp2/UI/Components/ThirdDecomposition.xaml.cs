using Orbifold.Graphite;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfApp2.Calc;
using WpfApp2.DB.Models;

namespace WpfApp2.UI.Components
{
    /// <summary>
    /// Логика взаимодействия для ThirdDecomposition.xaml
    /// </summary>
    public partial class ThirdDecomposition : UserControl, DataChangeNotifier
    {
        ProjectData data;
        NetCalculator calc;

        public ThirdDecomposition(ProjectData data)
        {
            this.data = data;

            InitializeComponent();

            calc = new NetCalculator(data, "A");

            var table = calc.generateEdgeDifferencetable();

            dtGrid.ItemsSource = table.DefaultView;
            showImage();
            updateGraph();

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
    }
}
