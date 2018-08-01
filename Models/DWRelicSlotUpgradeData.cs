using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWRelicSlotUpgradeDataInputParam
    {
        public string memberID;
        public string token;
    }

    [Serializable]
    public class DWRelicSlotUpgradeDataModel
    {
        public byte slotIdx;
        public long gem;
        public long cashGem;
        public byte errorCode;
    }
}