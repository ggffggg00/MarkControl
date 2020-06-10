using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WpfApp2.DB.Models;
using WpfApp2.UI.Windows;

namespace WpfApp2
{
    /// <summary>
    /// Окно создания проекта. Отображается в виде диалога и возвращает результат
    ///     Если проект успешно создан - True;
    ///     Если проект создать не удалось - False;
    ///     
    ///     При успешном создании проекта в поле insertedId записывается идентификатор созданного проекта
    /// </summary>
    public partial class CreateProjectScreen : Window
    {

        #region Поля
        /// <summary>
        /// Хранилище данных проекта
        /// </summary>
        private DatabaseHelper DBHelper;

        /// <summary>
        /// Переменная для хранения и проверки вводимых пользователем данных
        /// </summary>
        public ProjectInitialData InitialData;

        /// <summary>
        /// Идентификатор созданного проекта.
        /// После создания проекта сюда записывается идентификатор созданного проекта,
        /// только потом закрывается окно. Главное окно после закрытия проверяет этот идентификатор.
        /// Если идентификатор присвоен, загружает данные и отображает главное окно
        /// </summary>
        public long insertedId { get; set; } = 0;
        #endregion

        #region Конструктор
        public CreateProjectScreen(DatabaseHelper DBHelper)
        {
            this.DBHelper = DBHelper;
            this.InitialData = new ProjectInitialData();
            InitializeComponent();

            this.MSP.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Обработчики событий

        //Кнопка распределения марок по блокам
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            SortMarksInBlocks();
        }

        //Кнопка создания проекта
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            //Проверяем, все ли данные (кроме блоков) были введены. В противном случае выводим ошибку
            if (!InitialData.hasFullyInfo())
            {
                showAlert(@"Не все поля заполнены или не прикреплено изображение");
                return;
            }

            //Получаем ошибку в данных. Если она есть, то будет записан ее текст. В противном случае будет присвоен NULL
            string error = InitialData.checkDataValid();

            //Проверяем, есть ли ошибка. Если есть, отображаем ее и выходим из функции
            if (error != null)
            {
                showAlert(error);
                return;
            }

            //Выполняем запрос к БД на создание проекта и записываем в переменную идентификатор созданного проекта
            this.insertedId = new CreateProjectRequest(DBHelper, InitialData).execute();
            //Выполняем запрос в БД на создание таблицы для проекта
            new CreateProjectTableRequest(DBHelper, insertedId, Int32.Parse(InitialData.markCount)).execute();

            //Завершаем работу диалога с положительным результатом
            this.DialogResult = true;
            this.Close();
        }

        //кнопка отмены
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //Завершаем работу диалога с отрицательным результатом
            this.DialogResult = false;
            this.Close();
        }

        //Кнопка выбора изображения
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //Настраиваем диалог открытия файла 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "Изображение (.png)|*.png";
            dlg.Multiselect = false;

            //Открываем диалог и получаем его результат
            Nullable<bool> result = dlg.ShowDialog();

            //Если результат положительный, то записываем путь к файлу и показываем изображение
            if (result == true)
            {
                this.InitialData.imagePath = dlg.FileName;
                loadPreview(this.InitialData.imagePath);
            }

        }

        //При изменении текста записывает значения в объект данных
        private void TitleField_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            //Обрабатываем исключение. Оно появится, если данные не заполнены
            try
            {
                this.InitialData.title = TitleField.Text;
                this.InitialData.markCount = MarkCountField.Text;
                this.InitialData.blockCount = BlockCountField.Text;
            }
            catch (NullReferenceException ex)
            {
            }
        }

        //Позволяет перетаскивать окно за любую его часть
        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        #endregion

        #region Вспомогательные методы
        /// <summary>
        /// Показывает предупреждающее сообщение
        /// </summary>
        /// <param name="msg">Сообщение для вывода</param>
        private void showAlert(string msg)
        {
            MessageBox.Show(msg, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// Отображает схему фундамента
        /// </summary>
        /// <param name="path">Путь к файлу изображения</param>
        private void loadPreview(string path)
        {
            BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            bi3.UriSource = new Uri(path, UriKind.Absolute);
            bi3.EndInit();
            ImagePreview.Source = bi3;
        }


        /// <summary>
        /// Запускает процесс распределения блоков. Обрабатывает возможные ошибки
        /// </summary>
        private void SortMarksInBlocks()
        {
            //Получаем возможную ошибку при заполнении данных
            string error = InitialData.checkDataValid(false);

            //Если ошибка есть, выводим ее и выходим из функции
            if (error != null)
            {
                showAlert(error);
                return;
            }

            //Перегоняем строки в числа
            int bc = Int32.Parse(InitialData.blockCount);
            int mc = Int32.Parse(InitialData.markCount);

            //Читаем массив байтов из файла изображения
            byte[] imgBytes = File.ReadAllBytes(InitialData.imagePath);

            //Создаем диалог распределения марок
            BlockInputFormDialog dlg = new BlockInputFormDialog(bc, mc, imgBytes);

            //Если результат диалога НЕ положительный, то показываем сообщение и выходим
            if (dlg.ShowDialog() != true)
            {
                showAlert("Вы отменили распределение марок по блокам");
                return;
            }

            //Меняем ширину окна чтобы список поместился и делаем список видимым
            this.MSP.Visibility = Visibility.Visible;
            this.Width = 768;

            //Добавляем информацию о распределении блоков
            InitialData.addBlockInfo(dlg.Result);

            //Заполняем в цикле список блоков
            foreach (BlockObject obj in dlg.Result)
                LV.Items.Add(obj.ToString());

        }

        #endregion

        
    }
}
