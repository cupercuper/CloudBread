using System;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWRelicOpenInputParam
    {
        public string memberID;
        public string token;
    }

    [Serializable]
    public class DWRelicOpenModel
    {
        public RelicData relicData;
        public long ether;
        public long cashEther;
        public byte errorCode;
    }
}