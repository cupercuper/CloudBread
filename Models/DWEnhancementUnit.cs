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
        public long enhancedStone;
        public long cashEnhancedStone;
        public long gem;
        public long cashGem;
        public byte success;
        public byte errorCode;
    }
}