using System;
using DW.CommonData;

namespace CloudBread.Models
{

    [Serializable]
    public class DWRelicBoxOpenDataInputParam
    {
        public string memberID;
        public string token;
    }

    [Serializable]
    public class DWRelicBoxOpenDataModel
    {
        public RelicData relicData;
        public long relicBoxCount;
        public byte errorCode;
    }
}