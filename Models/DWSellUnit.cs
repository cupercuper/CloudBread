using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudBread.Models
{
    public class DWSellUnitInputParam
    {
        public string memberID;
        public uint instanceNo;
        public string token;
    }

    public class DWSellUnitModel
    {
        public uint InstanceNo;
        public int Gem;
        public int EnhancedStone;
        public byte ErrorCode;
    }
}