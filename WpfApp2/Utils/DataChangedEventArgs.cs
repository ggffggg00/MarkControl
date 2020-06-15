using System;


namespace WpfApp2.Utils
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
