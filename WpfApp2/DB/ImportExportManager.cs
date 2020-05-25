using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using WpfApp2.DB.Models;

namespace WpfApp2.DB
{
    class ImportExportManager
    {
        public static List<MarksRow> parseCsv(string filePath){

            List<MarksRow> list = new List<MarksRow>();

            using (var reader = new StreamReader(filePath))
            {
                
                int currEpoch = 0;
                while (!reader.EndOfStream)
                {
                    var values = reader.ReadLine().Split(',');
                    var mark = new MarksRow(currEpoch++);

                    for (int i = 0; i < values.Length; i++)
                        mark.addMark(i + 1, Double.Parse(values[i].Trim(), CultureInfo.InvariantCulture));

                    list.Add(mark);
                }
            }

            return list;

        }


    }
}
