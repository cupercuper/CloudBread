using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWTutorialSuccessDataInputParam
    {
        public string memberID;
        public byte serialNo;
        public string token;
    }

    [Serializable]
    public class DWTutorialSuccessDataModel
    {
        public byte errorCode;
    }
}