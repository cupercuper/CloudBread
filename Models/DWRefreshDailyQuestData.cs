using System;
using System.Collections.Generic;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWRefreshDailyQuestInputParams
    {
        public string memberID;
        public string token;
    }

    [Serializable]
    public class DWRefreshDailyQuestModel
    {
        public List<QuestData> dailyQuestList;
        public byte errorCode;
    }
}