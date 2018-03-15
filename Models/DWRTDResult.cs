using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWRTDResultInputParams
    {
        public string memberID;
        public long score;
        public string token;
    }

    [Serializable]
    public class DWRTDResultModel
    {
        public long rank;
        public byte errorCode;
    }
}