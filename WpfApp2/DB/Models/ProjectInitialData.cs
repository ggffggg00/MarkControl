using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace WpfApp2.DB.Models
{
    public class ProjectInitialData
    {
        public string title { get; set; }
        public string markCount {  get; set; }
        public string blockCount { get; set; }
        public string imagePath { get; set; }
        public bool hasBlockDataPresented { get; set; } = false ;
        public JObject metaData { get; set; } = new JObject();

        public bool hasFullyInfo()
        {
            return !(string.IsNullOrEmpty(title) || string.IsNullOrEmpty(markCount) || string.IsNullOrEmpty(blockCount) || string.IsNullOrEmpty(imagePath));
        }

        public void addBlockInfo(List<BlockObject> blocks)
        {
            JArray array = new JArray();
            foreach (BlockObject bl in blocks)
                array.Add(JObject.FromObject(bl));

            metaData.Add("blockData", array);
            hasBlockDataPresented = true;
        }

        public void eraseBlockInfo()
        {
            if (!hasBlockDataPresented)
                return;

            metaData.Remove("blockData");
            hasBlockDataPresented = false;
        }

        public string checkDataValid(bool checkBlockData = true)
        {
            if (!hasFullyInfo())
                return "Некоторые поля не заполнены";

            int markCoun, blockCoun;

            if (!int.TryParse(this.markCount, out markCoun) || !int.TryParse(this.blockCount, out blockCoun))
                return "Проверьте правильность введенных количества блоков и количества марок";

            if (blockCoun > markCoun / 2)
                return "Блоков не может быть больше, чем половина марок";

            if (!File.Exists(imagePath))
                return "Файл изображения не найден";

            if (!hasBlockDataPresented & checkBlockData)
                return "Распределение марок по блокам не выполнено";

            return null;
        }


    }
}
