using System;
using System.Collections.Generic;

namespace DW.CommonData
{
    public enum DW_ERROR_CODE
    {
        OK = 0,
        NOT_FOUND_USER,
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

    [Serializable]
    public class ClientUnitData
    {
        public uint instanceNo;
        public ushort level;
        public ushort enhancementCount;
        public ulong serialNo;
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
    }

}