using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWUseGameSpeedItemDataInputParams
    {
        public string memberID;
        public string token;
    }

    [Serializable]
    public class DWUseGameSpeedItemDataModel
    {
        public byte gameSpeedItemCnt;
        public long remainTime;
        public byte errorCode;
    }
}