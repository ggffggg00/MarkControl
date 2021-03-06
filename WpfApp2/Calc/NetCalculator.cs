using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WpfApp2.DB.Models;

namespace WpfApp2.Calc
{
    #region Производные классы
    class BlockData
    {
        public string Name { get; }
        public int[] Marks { get; }
        public int Count { get => Marks.Length; }
        public List<KeyValuePair<int, int>> Edges { get; } = new List<KeyValuePair<int, int>>();
        public List<KeyValuePair<int, int>> EdgeIndexes { get; } = new List<KeyValuePair<int, int>>();

        public BlockData(string name, int[] marks)
        {
            this.Name = name;
            this.Marks = marks;
            findEdges();
            findEdgeIndexes();
        }

        void findEdges()
        {
            for(int from = 0; from < Count; from++ )
                for(int to = from; to < Count; to++)
                    if (Marks[from] != Marks[to])
                        Edges.Add(new KeyValuePair<int, int>(Marks[from], Marks[to]));
                            
        }

        void findEdgeIndexes()
        {
            for (int from = 0; from < Count; from++)
                for (int to = from; to < Count; to++)
                    if (Marks[from] != Marks[to])
                        EdgeIndexes.Add(new KeyValuePair<int, int>(from, to));

        }



    }

    /// <summary>
    /// Реализовывает хранение в таблице значения ячейки и ее цвета
    /// </summary>
    public class ColoredDataGridCell
    {
        public object Value { get; set; }
        public Brush Color { get; set; }

        public ColoredDataGridCell(object value, Brush color)
        {
            Value = value;
            Color = color;
        }

        public ColoredDataGridCell() { }

        public override string ToString() => Value.ToString();

    }
    #endregion

    /// <summary>
    /// Класс для рассчетов и проверки связей между подблоками
    /// </summary>
    class NetCalculator
    {
        #region Поля
        /// <summary>
        /// Хранилище данных проекта
        /// </summary>
        private ProjectData ProjectData { get; }

        /// <summary>
        /// Ячейка для хранения данных о связях текущего блока
        /// </summary>
        public BlockData CurrentBlockData { get; set; }
        #endregion

        #region Конструктор
        public NetCalculator(ProjectData data, BlockObject currentBlock)
        {
            this.ProjectData = data;
            this.CurrentBlockData = parseBlock(currentBlock);
        }
        #endregion

        #region Публичные методы

        /// <summary>
        /// Проверяет, является ли связь абсолютно твердой
        /// </summary>
        /// <param name="edgeIndexes">Пара индексов рассматриваемых марок марок</param>
        /// <returns>Твердая ли связь</returns>
        public bool hasEdgeSolid(KeyValuePair<int, int> edgeIndexes)
        {
            int success = 0, failed = 0;
            foreach (int epoch in Enumerable.Range(0, ProjectData.epochCount))
            {
                bool isSolid = hasSingleEdgeSolid(calculateEdgeDifference(CurrentBlockData.Marks[edgeIndexes.Key], CurrentBlockData.Marks[edgeIndexes.Value], epoch));
                if (isSolid)
                    success++;
                else
                    failed++;
            }

            return success > failed;
        }

        /// <summary>
        /// Возвращает количество возможных связей в блоке
        /// Рассчитывается согласно формуле
        /// </summary>
        /// <returns>Количество возможных связей в блоке</returns>
        public int EdgesCount()
        {
            return (CurrentBlockData.Count * (CurrentBlockData.Count - 1)) / 2;
        }

        /// <summary>
        /// Возвращает достаточное количество абсолютно твердых связей
        /// Рассчитывается согласно формуле
        /// </summary>
        /// <returns></returns>
        public int EdgeCountForSolidBlock()
        {
            return 3 * CurrentBlockData.Count - 6;
        }

        /// <summary>
        /// Генерирует таблицу данных для отображения
        /// </summary>
        /// <returns>Таблица данных</returns>
        public DataTable generateEdgeDifferencetable()
        {
            var data = new DataTable();

            DataColumn epochCol = new DataColumn("Эпоха");
            epochCol.DataType = new ColoredDataGridCell().GetType();
            data.Columns.Add(epochCol);

            foreach (KeyValuePair<int, int> edge in CurrentBlockData.Edges)
            {
                DataColumn col = new DataColumn(edge.Key.ToString() + "-" + edge.Value.ToString());
                col.DataType = new ColoredDataGridCell().GetType();
                data.Columns.Add(col);
            }


            foreach (int epoch in Enumerable.Range(0, ProjectData.epochCount))
            {

                var converter = new System.Windows.Media.BrushConverter();
                var brush = (Brush)converter.ConvertFromString("#41416a");

                ColoredDataGridCell[] dt = new ColoredDataGridCell[CurrentBlockData.Edges.Count + 1];
                dt[0] = new ColoredDataGridCell(epoch, brush);
                int i = 1;
                foreach (KeyValuePair<int, int> edge in CurrentBlockData.Edges)
                {
                    Brush backColor = hasSingleEdgeSolid(calculateEdgeDifference(edge.Key, edge.Value, epoch)) ? Brushes.DarkGreen : Brushes.DarkRed;
                    dt[i++] = new ColoredDataGridCell(calculateEdgeDifference(edge.Key, edge.Value, epoch), backColor);
                }

                data.Rows.Add(dt);

            }

            return data;

        }
        #endregion

        #region Вспомогательные методы
        /// <summary>
        /// Метод позволяет разобрать блок на внутренний тип данных для блока для возможности хранения информации о связях
        /// </summary>
        /// <param name="block">Экземпляр блока</param>
        /// <returns></returns>
        private BlockData parseBlock(BlockObject block)
        {

            return new BlockData(
                block.blockName,
                block.marks);
        }

        /// <summary>
        /// Метод рассчитывает разницу между марками,индекс которых передан в параметры
        /// </summary>
        /// <param name="from"> Индекс марки начала </param>
        /// <param name="to"> Индекс марки конца </param>
        /// <param name="epoch"> Рассматриваемая эпоха </param>
        /// <returns>Разница между марками</returns>
        double calculateMarkDifference(int from, int to, int epoch)
        {
            return ProjectData.marks[epoch].marks[from] - ProjectData.marks[epoch].marks[to];
        }

        /// <summary>
        /// Возвращает ответ, не превышает ли разница между разницами марок допустимую ошибку
        /// </summary>
        /// <param name="difference"> Разница </param>
        /// <returns>Входит ли связь в допуск</returns>
        bool hasSingleEdgeSolid(double difference)
        {
            return ProjectData.eAccuracy >= Math.Abs(difference);
        }

        /// <summary>
        /// Рассчитывает разницу между разницей марки в эпохе и разницей марки в нулевой эпохе
        /// </summary>
        /// <param name="from">Связь ИЗ</param>
        /// <param name="to">Связь В</param>
        /// <param name="epoch">Индекс эпохи</param>
        /// <returns></returns>
        double calculateEdgeDifference(int from, int to, int epoch)
        {
            return calculateMarkDifference(from, to, epoch) - calculateMarkDifference(from, to, 0);
        }

        #endregion

    }
}
