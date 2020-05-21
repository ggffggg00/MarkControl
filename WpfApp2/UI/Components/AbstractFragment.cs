using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Text;
using System.Threading.Tasks;
using WpfApp2.DB.Models;

namespace WpfApp2.UI.Components
{
    /// <summary>
    /// Абстрактный класс для фрагментов программы. Реализовывает в себе методы обработки уведомдения при изменении данных
    /// </summary>
    public interface DataChangeNotifier
    {

        /// <summary>
        /// Реализация логики при изменении данных
        /// </summary>
        void onDataChanged(EventArgs e);

    }
}
