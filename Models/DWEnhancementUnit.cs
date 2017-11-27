using System;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWEnhancementUnitInputParam
    {
        public string memberID;
        public uint InstanceNo;
        public byte gemUse;
        public string token;
    }

    [Serializable]
    public class DWEnhancementUnitModel
    {
        public ClientUnitData unitData;
        public int enhancedStone;
        public int gem;
        public byte success;
        public byte errorCode;
    }
}