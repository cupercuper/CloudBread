﻿using System;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWEnhancementUnitInputParam
    {
        public string memberID;
        public uint InstanceNo;
        public string token;
        public byte enhancedCount;
    }

    [Serializable]
    public class DWEnhancementUnitModel
    {
        public ClientUnitData unitData;
        public int enhancedStone;
        public byte errorCode;
    }
}