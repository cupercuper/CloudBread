using System;
using System.Collections.Generic;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWOpenResouceDrillDataInputParams
    {
        public string memberID;
        public byte drillIdx;
        public string token;
    }

    [Serializable]
    public class DWOpenResouceDrillDataModel
    {
        public DWItemData itemData;
        public long ether;
        public long gas;
        public long gem;
        public double gold;
        public List<SkillItemData> skillItemList;
        public List<BoxData> boxList;
        public long relicBoxCnt;
        public bool droneAdvertisingOff;
        public byte errorCode;
    }
}