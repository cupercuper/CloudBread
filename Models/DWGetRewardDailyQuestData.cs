using System;
using System.Collections.Generic;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWGetRewardDailyQuestInputParam
    {
        public string memberID;
        public byte questIdx;
        public string token;
    }

    [Serializable]
    public class DWGetRewardDailyQuestModel
    {
        public double mineral;
        public long gem;
        public long ether;
        public long gas;
        public long relicBoxCnt;
        public List<SkillItemData> skillItemList;
        public List<BoxData> boxList;
        public bool droneAdvertisingOff;
        public byte errorCode;
    }
}