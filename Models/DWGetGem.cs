using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWGetGemInputParam
    {
        public string memberID;
        public long gem;
        public string token;
    }

    [Serializable]
    public class DWGetGemModel
    {
        public long gem;
        public byte errorCode;
    }

}