using System;
using System.Collections.Generic;
using DW.CommonData;

namespace CloudBread.Models
{
    public class DWUnitStoreBuyInputParam
    {
        public string memberID;
        public byte index;
        public string token;
    }

    public class DWUnitStoreBuyModel
    {
        public ClientUnitData unitData;
        public int enhancedStone;
        public byte index;
        public byte errorCode;
    }
}