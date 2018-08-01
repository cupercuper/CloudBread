using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWBossClearDataInputParam
    {
        public string memberID;
        public uint clearIdx;
        public string token;
    }

    [Serializable]
    public class DWBossClearDataModel
    {
        public byte errorCode;
    }
}