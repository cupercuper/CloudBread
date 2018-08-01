using System;
using System.Collections.Generic;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWReturnDataInputParams
    {
        public string memberID;
        public byte captainIdx;
        public string token;
    }

    [Serializable]
    public class DWReturnDataModel
    {
        public long ether;
        public long gas;
        public byte captainIdx;
        public List<UnitData> unitList;
        public long lastGasStageNo;
        public long lastReturnStageNo;
        public byte errorCode;
    }
}