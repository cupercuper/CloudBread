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
        public byte unitStore;
        public string token;
    }

    [Serializable]
    public class DWBuyUnitModel
    {
        public List<UnitStoreData> unitStoreDataList;
        public List<ClientUnitData> unitDataList;
        public long gem;
        public long cashGem;
        public long enhancedStone;
        public long cashEnhancedStone;
        public byte errorCode;
    }
}