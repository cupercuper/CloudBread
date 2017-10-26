using System;
using System.Collections.Generic;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWBuyUnitInputParam
    {
        public string memberID;
        public byte index;
        public string token;
    }

    [Serializable]
    public class DWBuyUnitModel
    {
        public ClientUnitData unitData;
        public int gem;
        public int enhancedStone;
        public byte errorCode;
    }
}