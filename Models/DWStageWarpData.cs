using System;
using System.Collections.Generic;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWStageWarpDataInputParam
    {
        public string memberID;
        public byte warpIdx;
        public string token;
    }

    [Serializable]
    public class DWStageWarpDataModel
    {
        public double mineral;
        public long gem;
        public long cashGem;
        public short warpWorldNo;
        public short warpStageNo;
        public List<UnitData> addUnitList;
        public byte errorCode;
    }
}