using System;
using System.Data;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace WpfApp2.Utils

{
    public class DataRowViewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DataGridCell cell && cell.DataContext is DataRowView drv)
                return drv.Row[cell.Column.SortMemberPath];
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
