using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWEnhancementResetInputParam
    {
        public string memberID;
        public uint instanceNo;
        public string token;
    }

    [Serializable]
    public class DWEnhancementResetModel
    {
        public uint instanceNo;
        public long ether;
        public byte errorCode;
    }
}