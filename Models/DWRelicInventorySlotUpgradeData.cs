using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWRelicInventorySlotUpgradeDataInputParam
    {
        public string memberID;
        public string token;
    }

    [Serializable]
    public class DWRelicInventorySlotUpgradeDataModel
    {
        public byte slotIdx;
        public long gem;
        public long cashGem;
        public byte errorCode;
    }
}