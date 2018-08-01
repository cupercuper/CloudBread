using System;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWEnhancementUnitInputParam
    {
        public string memberID;
        public uint InstanceNo;
        public ushort curEnhancementCnt;
        public string token;
    }

    [Serializable]
    public class DWEnhancementUnitModel
    {
        //public ClientUnitData unitData;
        public long ether;
        public long cashEther;
        public byte errorCode;
    }
}