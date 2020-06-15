
namespace WpfApp2.Utils
{
    /// <summary>
    /// Абстрактный класс для фрагментов программы. Реализовывает в себе методы обработки уведомдения при изменении данных
    /// </summary>
    public interface DataChangeNotifier
    {

        /// <summary>
        /// Реализация логики при изменении данных
        /// </summary>
        void onDataChanged(DataChangedEventArgs e);

    }
}
