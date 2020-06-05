using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WpfApp2.DB.Models;

namespace WpfApp2.UI.Windows
{
    /// <summary>
    /// Логика взаимодействия для BlockInputFormDialog.xaml
    /// </summary>
    public partial class BlockInputFormDialog : Window
    {

       
        public List<BlockObject> Result { get; set; }

        int maxMarksPerBlock;
        int minMarksPerBlock;
        int currentBlockIndex = 0;
        int[] marks;
        string[] blocks;

        List<int> currentBlockMarks = new List<int>();
        List<BlockObject> blocksList = new List<BlockObject>();

        ObservableCollection<int> listViewCollection = new ObservableCollection<int>();

        public BlockInputFormDialog(int blockCount, int marksCount, byte[] img) {
            Init(blockCount, Enumerable.Range(1,  marksCount).ToArray(), img);
        }

        public BlockInputFormDialog(int blockCount, int[] marks, byte[] img)
        {
            Init(blockCount, marks, img);   
        }

        void Init(int blockCount, int[] marks, byte[] imageData) {
                this.marks = marks;
                this.blocks = generateBlockArray(blockCount);
                this.minMarksPerBlock = marks.Length < 4 ? 1 : 2;
                this.maxMarksPerBlock = (int)Math.Ceiling((double)marks.Length / blockCount);

                InitializeComponent();
            
                fillOrUpdateList();
                updateIndicator();
                LV.ItemsSource = listViewCollection;
                ApplyButton.Visibility = Visibility.Hidden;

                showImage(imageData);
        }

        string[] generateBlockArray(int blockCount)
        {
            string[] arr = new string[blockCount];

            foreach (int currBlockIndex in Enumerable.Range(0, blockCount))
                arr[currBlockIndex] = ((char)(65 + currBlockIndex)).ToString();

            return arr;

        }

        void nextBlock() {
            if (!canNextBlock())
                return;

            var blockData = new BlockObject(blocks[currentBlockIndex], currentBlockMarks.ToArray());

            blocksList.Add(blockData);
            currentBlockMarks.Clear();

            currentBlockIndex++;

            if (currentBlockIndex == blocks.Length)
                finish();

            Add2BlockList(blockData);
            updateIndicator();
        }

        void finish()
        {
            ApplyButton.Visibility = Visibility.Visible;
        }


        bool canNextBlock()
        {
            return (currentBlockMarks.Count >= minMarksPerBlock && currentBlockMarks.Count <= maxMarksPerBlock);
        }

        void updateIndicator()
        {
            if (currentBlockIndex == blocks.Length)
                return;
            BI.Content = blocks[currentBlockIndex];
            MI.Content = String.Join(" ", currentBlockMarks.ToArray());
        }

        void fillOrUpdateList()
        {

            listViewCollection.Clear();

            foreach (int mark in marks)
                listViewCollection.Add(mark);
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (canNextBlock())
                nextBlock();
        }

        private void LV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (LV.SelectedItem == null)
                return;

            currentBlockMarks.Add((int)LV.SelectedItem);
            listViewCollection.Remove((int)LV.SelectedItem);

            updateIndicator();

            if (currentBlockMarks.Count == maxMarksPerBlock  || listViewCollection.Count == 0)
                nextBlock();

        }

        /// <summary>
        /// Загружает и показывает схему объекта
        /// </summary>
        void showImage(byte[] data)
        {

            byte[] buffer = data;
            System.Windows.Media.ImageSource result;
            using (var stream = new MemoryStream(buffer))
            {
                result = BitmapFrame.Create(
                    stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }

            img.Source = result;
        }

        void Add2BlockList(BlockObject bl) {
            LV2.Items.Add(bl.ToString()) ;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            blocksList.Clear();
            currentBlockIndex = 0;
            currentBlockMarks.Clear();
            LV2.Items.Clear();
            fillOrUpdateList();
            updateIndicator();
            ApplyButton.Visibility = Visibility.Hidden;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Result = blocksList;
            DialogResult = true;
            Close();
        }
    }
}
