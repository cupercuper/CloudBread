using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudBread.Models
{
    public class DWEnhancementUnitInputParam
    {
        public string memberID;
        public uint InstanceNo;
        public string token;
        public byte enhancedCount;
    }

    public class DWEnhancementUnitModel
    {
        public uint InstanceNo;
        public UnitData UnitData;
        public int EnhancedStone;
        public byte ErrorCode;
    }
}