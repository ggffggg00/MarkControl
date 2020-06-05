using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp2.DB.Models
{
    public class BlockObject
    {
        public string blockName { get; set; }
        public int[] marks { get; set; }

        public static BlockObject FromJson(JObject obj)
        {
            var marksJArray = (JArray)obj["marks"];
            var blockName = (string)obj["blockName"];
            int[] marks = marksJArray.Select(jv => (int)jv).ToArray();

            return new BlockObject(blockName, marks);

        }

        public BlockObject(string blockName, int[] marks)
        {
            this.blockName = blockName;
            this.marks = marks;
        }

        public override string ToString()
        {
            return "Блок " + this.blockName + ": " + String.Join(", ", this.marks);
        }
    }
}
