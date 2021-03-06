using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfApp2.DB.Models;
using WpfApp2.UI.Components;
using WpfApp2.UI.Windows;
using WpfApp2.Utils;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {

        DatabaseHelper db;
        List<DataChangeNotifier> notifiersList = new List<DataChangeNotifier>();
        bool selectionFlag = false;
        ProjectData projectData;

        bool isComponentsRegistered = false;
        
        /// <summary>
        /// Главное окно программы. Реализовывает нотификацию дочерних компонентов
        /// </summary>
        /// <param name="db"> Контекст подключения к БД </param>
        /// <param name="projectId"> Идентификатор выбранного проекта </param>
        /// 
        public MainWindow(DatabaseHelper db, int projectId)
        {
            this.db = db;
            InitializeComponent();

            projectData = new GetProjectDataRequest(db, projectId).execute();

            setDataUpToDate(true);

            DataExplorerFragment dtExp = new DataExplorerFragment(db, projectData);
            dtExp.dataChangeEvent += InvokeNotification;
            addTab("Обозреватель данных", dtExp);

            UpdateStatusBar();

            if (projectData.epochCount != 0)
                registerAllComponents();

        }


        void registerAllComponents() {
            if (isComponentsRegistered)
                return;

            registerNotifierComponent("Декомпозиция 1 ур.", new FirstDecomposition(projectData));
            registerNotifierComponent("Декомпозиция 2 ур.", new SecondDecoposition(projectData));
            registerNotifierComponent("Декомпозиция 3 ур.", new ThirdDecomposition(projectData));
            registerNotifierComponent("Декомпозиция 4 ур.", new FourthDecomposition(projectData));

            isComponentsRegistered = true;
        }

        /// <summary>
        /// Отработка события изменения данных
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InvokeNotification(object sender, DataChangedEventArgs e)
        {
            

            setDataUpToDate(e.isUpToDate);
            UpdateStatusBar();

            if( e.needToNotifyFragments )
            //Перебираем список всех подписчиков события и уведомляем
                foreach (DataChangeNotifier notifier in notifiersList)
                    notifier.onDataChanged(e);

            if (projectData.epochCount != 0)
                registerAllComponents();

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

        void UpdateStatusBar() {
            epochIndicator.Text = "Количество эпох: "+projectData.epochCount.ToString();
            Eindicator.Text = "E = "+projectData.eAccuracy.ToString();
            AIndicator.Text = "A = "+projectData.aAccuracy.ToString();
            projectNameIndicator.Text = projectData.name;
        }

        void setDataUpToDate(bool flag) {
            dataStateIndicator.Text = flag ? "Данные актуальны" : "Данные изменены, требуется запись в БД";
            dataStateIndicator.Background = flag ? Brushes.Green : Brushes.Red;
        }


        //Метод отвечает за создание вкладки с контентом
        void addTab(string title, object content) {

            TabItem item = new TabItem();
            item.Header = title;
            item.Content = content;
            item.Height = 50;
            item.Foreground = (Brush)(new BrushConverter().ConvertFromString("#d3dae3"));
            item.FontFamily = new FontFamily("Gotham Pro Light");
            item.FontSize = 14;
            item.MinWidth = 200;
            item.IsSelected = !selectionFlag;

            tabControl.Items.Add(item);

            selectionFlag = true;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            tabControl.Items.Clear();
            tabControl.InvalidateVisual();
            Window1 win = new Window1();
            win.Show();
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            new UserGuide().Show();
        }
    }
}
