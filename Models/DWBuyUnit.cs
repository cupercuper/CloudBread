using System;
using System.Collections.Generic;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWBuyUnitInputParam
    {
        public string memberID;
        public ulong serialNo;
        public ushort level;
        public string token;
    }

    [Serializable]
    public class DWBuyUnitModel
    {
        public UnitData unitData;
        public byte errorCode;
    }
}