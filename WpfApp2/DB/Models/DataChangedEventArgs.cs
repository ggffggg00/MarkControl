using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp2.DB.Models
{
    public class DataChangedEventArgs : EventArgs
    {
        public bool isUpToDate { get; set; }
        public bool needToNotifyFragments { get; }

        public DataChangedEventArgs(bool isUpToDate, bool needToNotifyFragments)
        {
            this.isUpToDate = isUpToDate;
            this.needToNotifyFragments = needToNotifyFragments;
        }
    }
}
