using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudBread.Models
{
    public class DWBuyUnitInputParam
    {
        public string memberID;
        public byte index;
        public string token;
    }

    public class DWBuyUnitModel
    {
        public UnitData UnitData;
        public int Gem;
        public int EnhancedStone;
        public byte ErrorCode;
    }
}