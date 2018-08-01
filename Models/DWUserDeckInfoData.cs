using System;
using System.Collections.Generic;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWUserDeckInfoInputParams
    {
        public string memberID;
        public string userMemberID;
        public string token;
    }

    [Serializable]
    public class DWUserDeckInfoModel
    {
        public string nickName;
        public List<UnitData> unitList;
        public byte errorCode;
    }
}