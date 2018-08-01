using System;
using System.Collections.Generic;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWGetUserDataInputParams
    {
        public string memberID;
        public string token;
    }

    [Serializable]
    public class DWGetUserDataModel
    {
        public List<DWUserData> userDataList = new List<DWUserData>();
        public byte refreshDailyQeust;
        public byte errorCode;
    }
}