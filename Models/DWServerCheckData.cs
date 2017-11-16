using System;
using System.Collections.Generic;

namespace CloudBread.Models
{
    [Serializable]
    public class DWServerCheckInputParam
    {
        public string token;
    }

    [Serializable]
    public class DWServerCheckModel
    {
        public byte serverCheckState;
        public List<long> startTime;
        public List<long> endTime;
        public byte errorCode;
    }
}