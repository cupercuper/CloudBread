using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWEventInputParams
    {
        public string memberID;
        public byte eventCheckType;
        public string token;
    }

    [Serializable]
    public class DWEventModel
    {
        public byte errorCode;
    }
}