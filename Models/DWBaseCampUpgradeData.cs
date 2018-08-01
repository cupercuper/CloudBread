using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWBaseCampUpgradeDataInputParam
    {
        public string memberID;
        public ulong serialNo;
        public string token;
    }

    [Serializable]
    public class DWBaseCampUpgradeDataModel
    {
        public ushort level;
        public long gas;
        public long cashGas;
        public byte errorCode;
    }
}