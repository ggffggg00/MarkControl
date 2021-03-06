using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace WpfApp2.DB.Models
{
    /// <summary>
    /// Представление всех данных о проекте. Класс содержит все необходимые данные проекта
    /// </summary>
    public class ProjectData
    {
        //------------------Публичные поля класса---------------------

        /// <summary>
        /// Идентификатор проекта
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Название проекта
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Байтовое представление изображения с схемой объекта
        /// </summary>
        public byte[] img { get; set; }

        /// <summary>
        /// Количество марок на объекте
        /// </summary>
        public int marksCount { get; set; } = 0 ;

        /// <summary>
        /// Количество блоков объекта
        /// </summary>
        public int blocksCount { get; set; } = 0;

        /// <summary>
        /// Величина ошибки E. Является мета-полем
        /// </summary>
        public double eAccuracy
        {
            get => (double)metaData["eAccuracy"];
            set => metaData["eAccuracy"] = value;
        }

        /// <summary>
        /// Коэфициент экспоненциального сглаживания (A). Является мета-полем
        /// </summary>
        public double aAccuracy
        {
            get => (double)metaData["aAccuracy"];
            set => metaData["aAccuracy"] = value;
        }

        public JArray markInBlockOrder
        {
            get => (JArray)metaData["blockData"];
            set => metaData["blockData"] = value;
        }


        /// <summary>
        /// Количество эпох в базе данных
        /// </summary>
        public int epochCount {
            get => this.marks.Count; 
        }

        /// <summary>
        /// Хрнаилище меток. Представляет собой список объектов MarksRow. Размер списка соответствует количеству эпох
        /// </summary>
        public List<MarksRow> marks { get; set; }


        //------------------------Приватные поля---------------------

        /// <summary>
        /// Поле содержит мета-данные проекта (динамическое хранилище данных).
        /// В базе данных данный объект хранится как JSON строка, зашифрованная в Base64
        /// При получении данных из БД запрос расшифровывает строку и приводит к типу JObject для возможности получения данных из него
        /// </summary>
        public JObject metaData { get; set; }

        //-----------------------------Статичные методы----------------------

        /// <summary>
        /// Генерирует объект ProjectData с данными о проекте из курсора SQL
        /// </summary>
        /// <param name="rd"> SQL курсор </param>
        /// <returns></returns>
        public static ProjectData conctruct(SQLiteDataReader rd) {

            
            var inst = new ProjectData();       //Создаем новый объект ProjectData для хранения данных проекта
            inst.marks = new List<MarksRow>();  //Создаем список эпох для хранения всех марок
            bool datawWrite = false;            //Флаг для индикации записи основных данных проекта. True - если данные уже записаны и перезапись не нужна


            while (rd.Read())                   //Проходим циклом по всем строкам таблицы
            {

                if (!datawWrite)                //Если основные данные не были записаны, 
                {
                    inst.id = rd.GetInt32(0);
                    inst.blocksCount = rd.GetInt32(4);
                    inst.marksCount = rd.GetInt32(2);
                    inst.name = rd.GetString(1);
                    var jsonMeta = Encoding.UTF8.GetString(Convert.FromBase64String(rd.GetString(5)));
                    inst.metaData = JObject.Parse(jsonMeta);
                    inst.img = Convert.FromBase64String(rd.GetString(3));
                }

                if (rd.IsDBNull(6))
                    break;

                inst.marks.Add(MarksRow.create(rd, inst.marksCount));
            }

            return inst;

        }

        internal bool SaveAllData(DatabaseHelper dbContext) {

            bool marksSaved = new UpdateProjectDataRequest(dbContext, this).execute();
            bool metaSaved = new UpdateProjectMetaRequest(dbContext, this).execute();

            return marksSaved && metaSaved;
        }

        internal void AddAllMarks(List<MarksRow> ls)
        {
            this.marks.AddRange(ls);
        }

        internal void addPredictedRow() {

            MarksRow row = new MarksRow(epochCount);

            for (int markIndex = 1; markIndex <= marksCount; markIndex++)
                row.addMark(markIndex, predictMarkValue(markIndex));

            this.marks.Add(row);
        }

        private double randomDouble(double max = 0, double min = 0) {
            Random random = new Random();
            return random.NextDouble() * (max - min) + min;

        }

        private double predictMarkValue(int markIndex) {
            double avg = 0;
            

            for (int i = 1; i < epochCount; i++)
            {
                double currVal = marks[i].marks[markIndex];
                double prevVal = marks[i - 1].marks[markIndex];
                avg += Math.Abs(currVal - prevVal);
            }

            avg = avg / (epochCount - 1);

            double zeroMark = marks[0].marks[markIndex];
            double randomSeed = randomDouble(avg, avg * (-1));

            return Math.Round(zeroMark + randomSeed, 4);

        }

    }


    /// <summary>
    /// Модель хранения данных марок для одной эпохи
    /// </summary>
    public class MarksRow
    {
        public int epoch { get;  }
        public Dictionary<int, double> marks { get; } = new Dictionary<int, double>();

        public MarksRow(int epoch = 0)
        {
            this.epoch = epoch;
        }


        public void addMark(int markIndex, double value) {
            marks.Add(markIndex, value);
        }

        public static MarksRow create(SQLiteDataReader rd, int marksCount)
        {

            int index = rd.GetOrdinal("epoch");
            var inst = new MarksRow(rd.GetInt32(index));

            for (int i = 1; i <= marksCount; i++){
                int ind = rd.GetOrdinal(i.ToString());
                inst.marks.Add(i, rd.GetDouble(ind));
            }


            return inst;
        }
    }

}
