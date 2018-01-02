using System;
using System.Collections.Generic;

namespace DW.CommonData
{
    public enum DW_ERROR_CODE
    {
        OK = 0,
        NOT_FOUND_USER,
        DB_ERROR,
        LOGIC_ERROR,
        PURCHAESE_ERROR_INTABLE,
        PURCHAESE_ERROR_VERIFY,
        PURCHAESE_ERROR_CANCEL
    }

    public enum MONEY_TYPE
    {
        GOLD_TYPE,
        GEM_TYPE,
        ENHANCEDSTONE_TYPE,
        ADVERTISING_TYPE,
        CASH_TYPE,
        MAX_TYPE
    }

    public enum SERVER_CHECK_TYPE
    {
        NOT_TYPE,
        REGISTER_TYPE,
        CHECKING_TYPE,
        MAX_TYPE
    }

    public enum ITEM_TYPE
    {
        GOLD_TYPE,
        GEM_TYPE,
        ENHANCEDSTONE_TYPE,
        UNIT_TYPE,
        UNIT_RANDOM_TYPE,
        AUTO_GET_ITEM_TYPE,
        SPEED_UP_TYPE,
        UNIT_ATTACK_COOLTIME_TYPE,
        MAX_TYPE
    }
    
    public enum EVENT_TYPE
    {
        LOGIN_TYPE,
        MAX_TYPE
    }

    public enum RANK_TYPE
    {
        TOP_RANK_TYPE,
        MY_CENTER_TYPE,
        MAX_TYPE
    }

    public enum ACTIVE_ITEM_TYPE
    {
        GAME_SPEED_UP,
        AUTO_GET_ITEM,
        UNIT_ATTACK_COOL_TIME,
        MAX_TYPE
    }

    public enum SHOP_TYPE
    {
        FREE_TYPE,
        GEM_TYPE,
        LIMIT_TYPE,
        MAX_TYPE
    }

    public enum UNIT_SUMMON_TICKET_TYPE
    {
        FIX_TYPE,
        RANDOM_TYPE,
        MAX_TYPE
    }

    [Serializable]
    public class ClientUnitData
    {
        public uint instanceNo;
        public ushort level;
        public ushort enhancementCount;
        public ulong serialNo;
    }

    [Serializable]
    public class UnitStoreData
    {
        public ulong serialNo;
        public int count;
    }

    [Serializable]
    public class DWUserData
    {
        public string memberID;
        public string nickName;
        public string recommenderID;
        public short captianLevel;
        public byte captianID;
        public long captianChange;
        public short lastWorld;
        public short curWorld;
        public short curStage;
        public List<ClientUnitData> unitList;
        public List<ulong> canBuyUnitList;
        public long gold;
        public long gem;
        public long cashGem;
        public long enhancedStone;
        public long cashEnhancedStone;
        public byte unitSlotIdx;
        public long unitListChangeTime;
        public byte unitStore;
        public List<UnitStoreData> unitStoreList;
        public bool allClear;
        public List<ActiveItemData> activeItemList;
        public List<LimitShopItemData> limitShopItemDataList;
        public List<DWUnitTicketData> unitTicketList;
    }

    [Serializable]
    public class DWItemData
    {
        public ulong itemNo;
        public int count;
    }

    [Serializable]
    public class DWMailData
    {
        public long index;
        public string senderID;
        public string receiveID;
        public string msg;
        public long createdAt;
        public List<DWItemData> itemData;
    }

    [Serializable]
    public class EventData
    {
        public string msg;
        public List<DWItemData> itemData;
    }

    [Serializable]
    public class DWRankData
    {
        public string memberID;
        public string nickName;
        public long rank;
        public double score;
    }

    [Serializable]
    public class ActiveItemData
    {
        public byte itemType;
        public int limitTime;
        public long startTime;
        public string value;
    }

    [Serializable]
    public class LimitShopItemData
    {
        public ulong serialNo;
        public byte count;
    }

    [Serializable]
    public class DWGoogleGooglePurchaseVerifyData
    {
        public string productId;
        public string purchasesToken;
        public string packageName;
    }

    [Serializable]
    public class DWUnitTicketData
    {
        public UNIT_SUMMON_TICKET_TYPE ticketType;
        public ulong serialNo;
    }
}