using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
        public ulong gemBoxNo;
        public byte errorCode;
    }
}