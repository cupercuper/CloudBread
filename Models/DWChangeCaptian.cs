using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudBread.Models
{
    public class DWChangeCaptianInputParam
    {
        public string memberID;
        public string token;
    }

    public class DWChangeCaptianModel
    {
        public byte CaptianID;
        public int EnhancedStone;
        public byte ErrorCode;
    }
}