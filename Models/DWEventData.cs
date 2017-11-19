using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudBread.Models
{
    [Serializable]
    public class DWEventInputParams
    {
        public string memberID;
        public byte eventCheckType;
        public string token;
    }

    [Serializable]
    public class DWEventModel
    {
        public byte errorCode;
    }
}