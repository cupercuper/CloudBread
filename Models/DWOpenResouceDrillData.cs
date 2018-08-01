using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWOpenResouceDrillDataInputParams
    {
        public string memberID;
        public byte drillIdx;
        public string token;
    }

    [Serializable]
    public class DWOpenResouceDrillDataModel
    {
        public byte errorCode;
    }
}