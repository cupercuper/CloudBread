using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWGetRewardAchievementInputParam
    {
        public string memberID;
        public byte achievementIdx;
        public string token;
    }

    [Serializable]
    public class DWGetRewardAchievementModel
    {
        public ulong nextSerialNo;
        public byte errorCode;
    }
}