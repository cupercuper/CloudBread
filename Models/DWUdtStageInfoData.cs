using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWUdtStageInfoInputParams
    {
        public string memberID;
        public short worldNo;
        public string token;
    }

    [Serializable]
    public class DWUdtStageInfoModel
    {
        public short worldNo;
        public byte errorCode;
    }
}