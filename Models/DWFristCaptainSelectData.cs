using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWFristCaptainSelectDataInputParam
    {
        public string memberID;
        public byte captainID;
        public string token;
    }

    [Serializable]
    public class DWFristCaptainSelectDataModel
    {
        public byte captainID;
        public byte errorCode;
    }
}