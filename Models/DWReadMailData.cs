using System;
using System.Collections.Generic;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWReadMailInputParams
    {
        public string memberID;
        public long index;
        public string token;
    }

    [Serializable]
    public class DWReadMailModel
    {
        public double gold;
        public long gem;
        public long ether;
        public long gas;
        public List<SkillItemData> skillItemList;
        public List<BoxData> boxList;
        public long relicBoxCnt;
        public long index;
        public List<DWItemData> itemList;
        public byte errorCode;
    }
}