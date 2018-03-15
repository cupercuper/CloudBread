﻿using System;
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

    public enum APP_VERSION_CHECK_TYPE
    {
        DIFFERENT_TYPE,
        SAME_TYPE,
        MAX_TYPE,
    }

    public enum ITEM_TYPE
    {
        GOLD_TYPE,
        GEM_TYPE,
        ENHANCEDSTONE_TYPE,
        UNIT_TYPE,
        UNIT_RANDOM_TYPE,
        ACTIVE_ITEM_TYPE,
        BOSS_DUNGEON_TICKET_TYPE,
        MAX_TYPE
    }
    
    public enum EVENT_TYPE
    {
        LOGIN_TYPE,
        MAX_TYPE
    }

    public enum RANK_SORT_TYPE
    {
        TOP_RANK_TYPE,
        MY_CENTER_TYPE,
        MAX_TYPE
    }

    public enum RANK_TYPE
    {
        CUR_STAGE_TYPE,
        ACC_STAGE_TYPE,
        RTD_MODE_TYPE,
        MAX_TYPE
    }

    public enum ACTIVE_ITEM_TYPE
    {
        GAME_SPEED_UP,
        GOLD_GET_INCREASE,
        GOLD_UP,
        MAX_TYPE
    }

    public enum SHOP_TYPE
    {
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

    public enum NICK_NAME_CHECK_TYPE
    {
        USE_TYPE = 0,
        SAME_TYPE,
        MAX_TYPE
    }

    public enum MAIL_ITEM_TYPE
    {
        ITEM_DATA_TYPE = 0,
        GOLD_TYPE,
        GEM_TYPE,
        ENHANCEDSTONE_TYPE,
        MAX_TYPE
    }

    public enum BOSS_DUNGEON_ENTER_TYPE
    {
        NORMAL_ENTER_TYPE = 0,
        GEM_ENTER_TYPE,
        MAX_TYPE
    }

    public enum MODE_TYPE
    {
        RANDOM_DEFENCE_TYPE = 0,
        MAX_TYPE,
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
        public short lastStage;
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
        public long accStage;
        public int bossDungeonTicket;
        public short curBossDungeonNo;
        public short lastBossDungeonNo;
    }

    [Serializable]
    public class DWItemData
    {
        public byte itemType;
        public int count;
    }

    [Serializable]
    public class DWMailData
    {
        public long index; 
        public string senderID;
        public string receiveID;
        public string title;
        public string msg;
        public long createdAt;
        public List<DWItemData> itemData;
    }

    [Serializable]
    public class EventData
    {
        public string title;
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
        public ulong serialNo;
        public int limitTime;
        public long startTime;
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