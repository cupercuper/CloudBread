using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWCompleteAchievementInputParam
    {
        public string memberID;
        public byte completeIdx;
        public string token;
    }

    [Serializable]
    public class DWCompleteAchievementModel
    {
        public byte errorCode;
    }
}