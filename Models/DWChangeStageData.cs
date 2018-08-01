using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWChangeStageInputParam
    {
        public string memberID;
        public string token;
        public int stageIdx;
        public bool allClear;
    }

    [Serializable]
    public class DWChangeStageModel
    {
        public ulong droneNo;
        public byte errorCode;
    }
}