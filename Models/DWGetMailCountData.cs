using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWGetMailCountInputParam
    {
        public string memberID;
        public string token;

    }

    [Serializable]
    public class DWGetMailCountModel
    {
        public int count;
        public byte errorCode;
    }
}