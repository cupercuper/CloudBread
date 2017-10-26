using System;
using System.Collections.Generic;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWInsUserDataInputParams
    {
        public string memberID;
        public string nickName;
        public string recommenderID;
        public string token;
    }

    [Serializable]
    public class DWInsUserDataModel
    {
        public DWUserData userData;
        public byte errorCode;
    }
}