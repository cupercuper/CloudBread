using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWScienceDroneDestroyDataInputParam
    {
        public string memberID;
        public string token;
    }

    [Serializable]
    public class DWScienceDroneDestroyDataModel
    {
        public byte errorCode;
    }
}