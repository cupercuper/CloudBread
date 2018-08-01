using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWCompleteDailyQuestInputParam
    {
        public string memberID;
        public byte completeIdx;
        public string token;
    }

    [Serializable]
    public class DWCompleteDailyQuestModel
    {
        public byte errorCode;
    }
}