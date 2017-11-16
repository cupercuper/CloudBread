using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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