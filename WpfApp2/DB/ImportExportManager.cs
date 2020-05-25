using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using WpfApp2.DB.Models;

namespace WpfApp2.DB
{
    class ImportExportManager
    {
        public static List<MarksRow> parseCsv(string filePath, int currEpoch = 0){

            List<MarksRow> list = new List<MarksRow>();

            using (var reader = new StreamReader(filePath))
            {
                
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


        public static void exportCsv(List<MarksRow> marks, string filePath, bool exportColumns = false)
        {
            var csv = new StringBuilder();

            if( exportColumns )
                csv.AppendLine(String.Join(";", marks[0].marks.Values));

            foreach (MarksRow mark in marks)
            {
                var row = String.Join(";", mark.marks.Values);
                csv.AppendLine(row);
            }

            File.WriteAllText(filePath, csv.ToString());

        }



    }
}
