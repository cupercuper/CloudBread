using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWRelicDestroyInputParam
    {
        public string memberID;
        public uint instanceNo;
        public string token;
    }

    [Serializable]
    public class DWRelicDestroyModel
    {
        public uint instanceNo;
        public byte errorCode;
    }
}