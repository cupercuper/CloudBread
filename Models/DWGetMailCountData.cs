using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudBread.Models
{
    [Serializable]
    public class DWGetMailCountInputParam
    {
        public string memberID;
        public string token;

    }

    [Serializable]
    public class DWGetMailCountModel
    {
        public int count;
        public byte errorCode;
    }
}