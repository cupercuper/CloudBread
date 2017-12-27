using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWGooglePurchaseVerifyInputParam
    {
        public string memberID;
        public string productId;
        public List<DWGoogleGooglePurchaseVerifyData> purchasesList;
        public string token;
    }

    [Serializable]
    public class DWGooglePurchaseVerifyModel
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