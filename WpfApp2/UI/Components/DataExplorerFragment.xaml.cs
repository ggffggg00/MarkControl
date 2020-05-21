
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Controls;

using WpfApp2.DB.Models;

namespace WpfApp2.UI.Components
{

    public partial class DataExplorerFragment : UserControl
    {
        DatabaseHelper db;
        ProjectData data;

        /// <summary>
        /// Экземпляр EventHandler для оповещения всех компонентов об изменении данных
        /// </summary>
        public event EventHandler<EventArgs> dataChangeEvent;

        public DataExplorerFragment(DatabaseHelper DB, ProjectData prData)
        {
            this.db = DB;
            this.data = prData;
            InitializeComponent();
            initDataGrid();

        }

        void initDataGrid()
        {

            DataTable Data = new DataTable();
            Data.Columns.Add("Эпоха");

            for (int i = 1; i <= data.marksCount; i++)
                Data.Columns.Add(i.ToString());

            foreach (MarksRow epoch in data.marks)
            {
                object[] values = new object[data.marksCount + 1];
                values[0] = epoch.epoch;

                foreach (KeyValuePair<int, double> markvalue in epoch.marks)
                    values[markvalue.Key] = markvalue.Value;

                Data.Rows.Add(values);


            }

            dtGrid.DataContext = Data.DefaultView;
                       


        }

        void notifyOnDataChanged() {
            EventHandler<EventArgs> handler = dataChangeEvent;
            if (handler != null)
                handler(this, new EventArgs());
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            notifyOnDataChanged();
        }
    }
}
