using System;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWRelicSellInputParam
    {
        public string memberID;
        public uint instanceNo;
        public string token;
    }

    [Serializable]
    public class DWRelicSellModel
    {
        public long ether;
        public long gem;
        public long cashGem;
        public uint inputInstanceNo;
        public RelicData relicStoreData;
        public byte errorCode;
    }
}