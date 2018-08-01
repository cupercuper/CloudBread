using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWRelicUpgradeInputParam
    {
        public string memberID;
        public uint instanceNo;
        public ushort levelCnt;
        public string token;
    }

    [Serializable]
    public class DWRelicUpgradeModel
    {
        public uint instanceNo;
        public ushort level;
        public long ether;
        public long cashEther;
        public byte errorCode;
    }
}