using System;
using System.Collections.Generic;

namespace DW.CommonData
{
    public enum DW_ERROR_CODE
    {
        OK = 0,
        NOT_FOUND_USER,
        DB_ERROR,
        LOGIC_ERROR
    }

    public enum MONEY_TYPE
    {
        GOLD_TYPE,
        GEM_TYPE,
        ENHANCEDSTONE_TYPE,
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
        public byte captianChange;
        public short lastWorld;
        public short curWorld;
        public short curStage;
        public List<ClientUnitData> unitList;
        public List<ulong> canBuyUnitList;
        public int gold;
        public int gem;
        public int enhancedStone;
        public byte unitSlotIdx;
        public long unitListChangeTime;
        public byte unitStore;
        public List<UnitStoreData> unitStoreList;
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
}