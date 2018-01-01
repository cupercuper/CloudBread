using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWGemBoxOpenInputParam
    {
        public string memberID;
        public string token;
    }

    [Serializable]
    public class DWGemBoxOpenModel
    {
        public long gem;
        public byte errorCode;
    }
}