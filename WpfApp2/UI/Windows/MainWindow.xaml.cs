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
using WpfApp2.UI.Components;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {

        DatabaseHelper db;
        int id;
        List<DataChangeNotifier> notifiersList = new List<DataChangeNotifier>();
        bool selectionFlag = false;
        
        /// <summary>
        /// Главное окно программы. Реализовывает нотификацию дочерних компонентов
        /// </summary>
        /// <param name="db"> Контекст подключения к БД </param>
        /// <param name="projectId"> Идентификатор выбранного проекта </param>
        /// 
        public MainWindow(DatabaseHelper db, int projectId)
        {
            this.db = db;
            this.id = projectId;
            InitializeComponent();

            var projectData = new GetProjectDataRequest(db, id).execute();

            DataExplorerFragment dtExp = new DataExplorerFragment(db, projectData);
            dtExp.dataChangeEvent += InvokeNotification;

            addTab("Обозреватель данных", dtExp);
            registerNotifierComponent("Декомпозиция 1", new FirstDecomposition(projectData));

        }

        /// <summary>
        /// Отработка события изменения данных
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InvokeNotification(object sender, EventArgs e)
        {
            //Перебираем список всех подписчиков события и уведомляем
            foreach (DataChangeNotifier notifier in notifiersList)
                notifier.onDataChanged(e);
        }


        /// <summary>
        /// Метод для добавления вкладки с контентом с предварительной регистрацией для уведомления об изменениях
        /// </summary>
        /// <param name="title"> Название вкладки </param>
        /// <param name="content"> Компонент для отображения внутри вкладки </param>
        /// 
        void registerNotifierComponent(string title, object content)
        {
            if (!(content is DataChangeNotifier))
                throw new NotImplementedException("AbstractFragment интерфейс не реализован!");

            if (!(content is UserControl))
                throw new NotImplementedException("Контент вкладки не имеет тип UserControl!");

            notifiersList.Add((DataChangeNotifier)content);
            addTab(title, content);
        }


        //Метод отвечает за создание вкладки с контентом
        void addTab(string title, object content) {

            TabItem item = new TabItem();
            item.Header = title;
            item.Content = content;
            item.Height = 50;
            item.Foreground = (Brush)(new System.Windows.Media.BrushConverter().ConvertFromString("#d3dae3"));
            item.FontFamily = new FontFamily("Gotham Pro Light");
            item.FontSize = 14;
            item.MinWidth = 200;
            item.IsSelected = !selectionFlag;

            tabControl.Items.Add(item);

            selectionFlag = true;

        }

    }
}
