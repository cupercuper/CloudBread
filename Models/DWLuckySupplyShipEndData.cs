using System;
using System.Collections.Generic;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWLuckySupplyShipEndInputParam
    {
        public string memberID;
        public string token;
    }

    [Serializable]
    public class DWLuckySupplyShipEndModel
    {
        public double gold;
        public long gem;
        public long ether;
        public long gas;
        public List<SkillItemData> skillItemList;
        public List<BoxData> boxList;
        public long relicBoxCnt;
        public List<DWItemData> itemList;
        public byte errorCode;
    }
}