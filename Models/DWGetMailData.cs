using System;
using System.Collections.Generic;
using DW.CommonData;


namespace CloudBread.Models
{
    [Serializable]
    public class DWGetMailInputParam
    {
        public string memberID;
        public int startIndex;
        public int offset;
        public string token;
    }

    [Serializable]
    public class DWGetMailModel
    {
        public List<DWMailData> mailList;
        public byte errorCode;
    }

}