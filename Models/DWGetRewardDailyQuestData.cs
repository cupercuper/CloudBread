using System;

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
        public byte errorCode;
    }
}