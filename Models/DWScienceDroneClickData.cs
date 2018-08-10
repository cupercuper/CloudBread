using System;
using System.Collections.Generic;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWScienceDroneClickDataInputParam
    {
        public string memberID;
        public ulong droneNo;
        public string token;
    }

    [Serializable]
    public class DWScienceDroneClickDataModel
    {
        public double gold;
        public long gem;
        public long ether;
        public long gas;
        public List<SkillItemData> skillItemList;
        public List<BoxData> boxList;
        public long relicBoxCnt;
        public bool droneAdvertisingOff;
        public DWItemData itemData;
        public byte errorCode;
    }
}