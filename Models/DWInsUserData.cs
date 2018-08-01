using System;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWInsUserDataInputParams
    {
        public string memberID;
        public string nickName;
        public string recommenderID;
        public string timeZoneID;
        public int timeZoneTotalMin;
        public string token;
    }

    [Serializable]
    public class DWInsUserDataModel
    {
        public DWUserData userData;
        public byte errorCode;
    }
}