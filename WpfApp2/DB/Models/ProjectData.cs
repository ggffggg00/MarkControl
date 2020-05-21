using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp2.DB.Models
{
    public class ProjectData
    {
        public int id { get; set; }
        public string name { get; set; }
        public byte[] img { get; set; }
        public int marksCount { get; set; } = 0 ;
        public int blocksCount { get; set; } = 0;
        public double eAccuracy { get; set; } = 0;
        public int epochCount { get; set; } = 0;
        public List<MarksRow> marks { get; set; }

        public static ProjectData conctruct(SQLiteDataReader rd) {

            var inst = new ProjectData();
            inst.marks = new List<MarksRow>();
            bool datawWrite = false;


            while (rd.Read())
            {

                if (!datawWrite)
                {
                    inst.id = rd.GetInt32(0);
                    inst.blocksCount = rd.GetInt32(4);
                    inst.marksCount = rd.GetInt32(2);
                    inst.name = rd.GetString(1);
                    //TODO set image
                }
                inst.epochCount++;
                inst.marks.Add(MarksRow.create(rd, inst.marksCount));
            }

            return inst;

        }

    }

    public struct MarksRow
    {
        public int epoch { get; set; }
        public Dictionary<int, double> marks { get; set; }

        public static MarksRow create(SQLiteDataReader rd, int marksCount)
        {
            var inst = new MarksRow();
            inst.marks = new Dictionary<int, double>();

            for (int i = 1; i <= marksCount; i++){
                int ind = rd.GetOrdinal(i.ToString());
                inst.marks.Add(i, rd.GetDouble(ind));
            }

            int index = rd.GetOrdinal("epoch");
            inst.epoch = rd.GetInt32(index);

            return inst;
        }
    }

}
