using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp2.UI.Windows;

namespace WpfApp2
{

    public partial class Window1 : Window
    {
        DatabaseHelper database;
        ObservableCollection<ProjectShortEntry> list {get; set;}

        public Window1()
        {
            this.database = new DatabaseHelper();
            list = new ProjectsListRequest(database).execute();

            InitializeComponent();

            this.ProjectList.ItemsSource = list;
            if (list.Count == 0)
                this.minifyProjectsList();

            //BlockInputFormDialog main = new BlockInputFormDialog(4, Enumerable.Range(1,12).ToArray());
            //main.Show();
            //this.Close();
        }


        private void refreshList() {
            list = new ProjectsListRequest(database).execute();
            this.ProjectList.DataContext = list;
            this.ProjectList.ItemsSource = list;
        }

       private void minifyProjectsList() {
            this.Width = this.Width - this.ListGrid.MaxWidth;
            this.MainGrid.Margin = new Thickness(10, 10, 10, 10);
            this.ListGrid.Visibility = Visibility.Collapsed;
        }

        private void openMainWindow(long projectId) {
            MainWindow main = new MainWindow(this.database, (int)projectId);
            main.Show();
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CreateProjectScreen createProject = new CreateProjectScreen(this.database);
            createProject.ShowDialog();
            if (createProject.DialogResult == true)
            {
                refreshList();
                long id = createProject.insertedId;
                createProject = null;
                openMainWindow(id);
            }
                
        }


        private void Image_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var image = sender as Image;
            if (image != null)
            {
                System.Console.Out.WriteLine("Hello!");
                System.Console.Out.WriteLine(image.Tag);
            }
        }

        private void ProjectList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProjectShortEntry pr = (ProjectShortEntry)ProjectList.SelectedItem;
            openMainWindow(pr.id);
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_MouseDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            new UserGuide().Show();
        }
    }
}
