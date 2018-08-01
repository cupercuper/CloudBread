using System;
using System.Collections.Generic;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWAchievementSyncInputParam
    {
        public string memberID;
        public List<QuestData> achievementSyncList;
        public string token;
    }

    [Serializable]
    public class DWAchievementSyncModel
    {
        public byte errorCode;
    }
}