using System;
using System.Collections.Generic;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWShopInputParam
    {
        public string memberID;
        public ulong serialNo;
        public string token;
    }

    [Serializable]
    public class DWShopModel
    {
        public long gold;
        public long gem;
        public long cashGem;
        public long enhancedStone;
        public long cashEnhancedStone;
        public List<ActiveItemData> activeItemList;
        public List<ClientUnitData> unitDataList;
        public List<DWUnitTicketData> unitTicketList;
        public byte errorCode;
    }
}