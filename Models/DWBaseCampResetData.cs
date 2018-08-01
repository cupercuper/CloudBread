using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWBaseCampResetDataInputParam
    {
        public string memberID;
        public string token;
    }

    [Serializable]
    public class DWBaseCampResetDataModel
    {
        public long gas;
        public long cashGas;
        public long gem;
        public long cashGem;
        public byte errorCode;
    }

}