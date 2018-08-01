using System;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWRelicBuyDataInputParam
    {
        public string memberID;
        public uint instanceNo;
        public string token;
    }

    [Serializable]
    public class DWRelicBuyDataModel
    {
        public long ether;
        public long cashEther;
        public ulong inputInstanceNo;
        public RelicData relicListData;
        public byte errorCode;
    }
}