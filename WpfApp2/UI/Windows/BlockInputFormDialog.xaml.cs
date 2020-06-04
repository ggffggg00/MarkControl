using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp2.UI.Windows
{
    /// <summary>
    /// Логика взаимодействия для BlockInputFormDialog.xaml
    /// </summary>
    public partial class BlockInputFormDialog : Window
    {

        class BlockObject
        {
            public string blockName { get; set; }
            public int[] marks { get; set; }

            public BlockObject(string blockName, int[] marks)
            {
                this.blockName = blockName;
                this.marks = marks;
            }
        }
        JArray Result;

        int maxMarksPerBlock;
        int minMarksPerBlock;
        int currentBlockIndex = 0;
        int[] marks;
        string[] blocks;

        List<int> currentBlockMarks = new List<int>();
        List<BlockObject> blocksList = new List<BlockObject>();

        ObservableCollection<int> listViewCollection = new ObservableCollection<int>();

        public BlockInputFormDialog(int blockCount, int marksCount) {
            Init(blockCount, Enumerable.Range(1,  marksCount).ToArray());
        }


        public BlockInputFormDialog(int blockCount, int[] marks)
        {
            Init(blockCount, marks);   
        }

        void Init(int blockCount, int[] marks) {
                this.marks = marks;
                this.blocks = generateBlockArray(blockCount);
                this.minMarksPerBlock = marks.Length < 4 ? 1 : 2;
                this.maxMarksPerBlock = (int)Math.Ceiling((double)marks.Length / blockCount);

                InitializeComponent();
                fillOrUpdateList();
                updateIndicator();
                LV.ItemsSource = listViewCollection;
                ApplyButton.Visibility = Visibility.Hidden;
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

        void Add2BlockList(BlockObject bl) {
            string s = "Блок " + bl.blockName + ": " + String.Join(", ", bl.marks);
            LV2.Items.Add(s);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            blocksList.Clear();
            currentBlockIndex = 0;
            currentBlockMarks.Clear();
            fillOrUpdateList();
            updateIndicator();
            ApplyButton.Visibility = Visibility.Hidden;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Result = new JArray();

            foreach (BlockObject block in blocksList)
                Result.Add(JObject.FromObject(block));

            DialogResult = true;
        }
    }
}
