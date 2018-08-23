using System;
using System.Collections.Generic;

namespace DW.CommonData
{
    public class RedisIndex
    {
        public static int LUCKY_SUPPLY_SHIP_RANK_IDX = 1000;
        public static int ATTENDANCE_RANK_IDX = 100;
    }

    public enum DW_ERROR_CODE
    {
        OK = 0,
        NOT_FOUND_USER,
        DB_ERROR,
        LOGIC_ERROR,
        PURCHAESE_ERROR_INTABLE,
        PURCHAESE_ERROR_VERIFY,
        PURCHAESE_ERROR_CANCEL,
        INSTANCE_NO_OVER,
    }

    public enum MONEY_TYPE
    {
        MINERAL_TYPE,
        GEM_TYPE,
        ETHER_TYPE,
        GAS_TYPE,
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
        ETHER_TYPE,
        SKILL_ITEM_TYPE,
        BOX_TYPE,
        GAS_TYPE,
        MINERAL_BOX_TYPE,
        RELIC_BOX_TYPE,
        DRONE_ADVERTISING_OFF_TYPE,
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
        MAX_TYPEc
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

    //public enum SHOP_TYPE
    //{
    //    GEM_TYPE,
    //    LIMIT_TYPE,
    //    MAX_TYPE
    //}

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

    public enum UNIT_CHANGE_TYPE
    {
        ADD_TYPE = 0,
        SUB_TYPE,
        CHANGE_TYPE,
        MAX_TYPE,
    }

    public enum DAILY_QUEST_GRADE_TYPE
    {
        GRADE_1 = 1,
        GRADE_2,
        GRADE_3,
        GRADE_4,
        GRADE_5,
        GRADE_ADD_REWARD,
        MAX_TYPE
    }

    public enum QUEST_TYPE
    {
        DIE_MONSTER = 0,//    몬스터 죽이기
        GET_MINERAL,// 미네랄 모으기
        CLEAR_STAGE,// 특정 스테이이지 통과
        GET_RELIC,// 유물 모으기
        UNIT_DPS,//    유닛 DPS 도달
        DIE_BOSS_COUNT,//보스 죽이기
        PLAY_RETURN,// 귀환 하기
        LEVEL_UP_UNIT,//    유닛 레벨업
        OPEN_BOX,//    상자 열기
        CRITICAL_ATTACK,//     크리티컬 공격 하기
        OPEN_UNIT,//    유닛 잠금 해제
        LEVEL_UP_CAPTAIN,//    지휘관 레벨업 하기
        UPGRADE_CAPTAIN,//    지휘관 특성 강화
        USE_SKILL_ITEM,//    스킬 사용
        CLEAR_DUNGEON,// 던젼 클리어
        UPGRADE_RELIC,//    유물 레벨업
        USE_RESOURCE_DRILL,//    자원드릴 이용
        USE_SHIP,//    룰렛 이용
        ATTENDANCE_CHECK,// Attendance    출석 하기
        PLAY_ADVERTISING,//    광고 시청하기
        CASH_SHOP,//구매하기
        OPEN_UNIT_COUNT,//    유닛 몇개 잠금 해재 
        CLEAR_STAGE_COUNT,// 몇 스테이이지  통과
        MAX_TYPE
    };

    public enum SKILL_ITEM_TYPE
    {
        STIMPACK_TYPE,
        NUCLEAR_ATTACK_TYPE,
        FLOATING_AIM_TYPE,
        LOCK_DOWN_TYPE,
        DADIOACTIVE_TYPE,
        MAX_TYPE
    };
    
    public enum BOX_TYPE
    {
        NORMAL_TYPE,
        BRONZE_TYPE,
        SILVER_TYPE,
        GOLD_TYPE,
        MAX_TYPE
    };

    public enum MINERAL_BOX_TYPE
    {
        NORMAL_TYPE,
        HIGH_TYPE,
        SHOP_NORMAL_TYPE,
        SHOP_HIGH_TYPE,
        MAX_TYPE
    }

    public enum UNIT_ATTACK_TYPE
    {
        FLAME_TYPE,
        RIFLE_TYPE,
        MACHINE_GUN_TYPE,
        CANNON_TYPE,
        MISSILE,
        MAX_TYPE
    }

    public enum UNIT_TYPE
    {
        HUMAN_TYPE,
        MACHINE_TYPE,
        TOWER_TYPE,
        MAX_TYPE
    }

    public enum DEFENCE_TYPE
    {
        SMALL_TYPE,
        MIDDLE_TYPE,
        BIG_TYPE,
        BOSS_TYPE,
        MAX_TYPE
    }

    public enum CAPTAIN_TYPE
    {
        HUMAN_TYPE,
        MACHINE_TYPE,
        TOWER_TYPE,
        MAX_TYPE
    }

    public enum BUFF_TYPE
    {
        UNIT_ATTACK, //공격력 증가
        UNIT_COOL_TIME, //공격속도 증가
        UNIT_CRITICLA_DAMAGE, //치명타 데미지 증가
        UNIT_CRITICLA_RATE, //치명타 확률 증가
        UNIT_GET_MINERAL, //획듣 미네랄량 증가
        UNIT_BOSS_GET_MINERAL, //획듣 미네랄량 증가
        MONSTER_DEFENCE, //몬스터 어력 감소
        MONSTER_MOVE_SPEED, //몬스터 이동속도 감소
        SKILL_EFFECT, //스킬 효과 증가
        CAPTAIN_UPGRADE_MONEY,    // 지휘관 업그레이드 비용 감소
        BOSS_MONSTER_TIME,    //보스 몬스터 공략 시간 증가
        UNIT_UPGRADE_MONEY,   //유닛 업그레이드 비용 감소
        DRONE_STAY_TIME,    //드론 소환 지속 시간 증가 
        GAME_SPEED_2X_TIME,   //2배속 지속 시간 증가
        RETURN_ETHER,  // 귀환시 에테르 획득량 증가
        OFF_LINE_MINERAL, // 휴식중 미네랄량 증가
        LUCKY_SHIP_MINERAL,    //행운의 보급선 미네랄량 증가
        RETURN_GAS, //귀환시 가스 획득량 증가
        RETURN_STAGE, // 귀환시 환생 스테이지 증가
        MONSTER_FREEZE_TYPE, // 몬스터를 멈춰 놓는다.
        MAX_TYPE
    };

    public enum BUFF_UNIT_TARGET_TYPE
    {
        ALL_TYPE = 0,
        ATTACK_TYPE,
        UNIT_TYPE,
        OWNER_TYPE,
        MAX_TYPE
    };

    public enum BUFF_MERGE_TYPE
    {
        ADD_TYPE,
        REPLACE_TYPE,
        MAX_TYPE
    };

    [Serializable]
    public class UnitData
    {
        public ushort level;
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
        public long captianChange;
        public short lastWorld;
        public short curWorld;
        public short curStage;
        public short lastStage;
        public List<UnitData> unitList;
        public double gold;
        public long gem;
        public long cashGem;
        public long ether;
        public long cashEther;
        public long gas;
        public long cashGas;
        public bool allClear;
        public List<ActiveItemData> activeItemList;
        public List<LimitShopItemData> limitShopItemDataList;
        public long accStage;
        public int bossDungeonTicket;
        public short curBossDungeonNo;
        public short lastBossDungeonNo;
        public List<uint> bossClearList;
        public string timeZoneID;
        public int timeZoneTotalMin;
        public byte continueAttendanceCnt;
        public byte accAttendanceCnt;
        public short continueAttendanceNo;
        public short accAttendanceNo;
        public List<QuestData> dailyQuestList;
        public long dailyQeustTimeRemain;
        public List<QuestData> achievementList;
        public byte resourceDrillIdx;
        public long resourceDrillTimeRemain;
        public long luckySupplyShipTimeRemain;
        public List<SkillItemData> skillItemList;
        public List<BoxData> boxList;
        public List<RelicData> relicList;
        public List<RelicData> relicStoreList;
        public byte relicSlotIdx;
        public List<BaseCampData> baseCampList;
        public long relicBoxCnt;
        public short gameSpeedItemCnt;
        public long gameSpeedItemTimeRemain;
        public long lastReturnStage;
        public long baseCampResetCnt;
        public byte relicInventorySlotIdx;
        public bool droneAdvertisingOff;
        public List<byte> tutorialSuccessList;
        public long freeBoxTimeRemain;
    }

    [Serializable]
    public class DWItemData
    {
        public byte itemType;
        public byte subType;
        public string value;
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

    [Serializable]
    public class QuestData
    {
        public ulong serialNo;
        public byte complete;
        public byte getReward;
        public string curValue;
    }

    [Serializable]
    public class LuckySupplyShipData
    {
        public byte shipIdx;
        public byte fail;
        public List<DWItemData> itemList;
    }

    [Serializable]
    public class RelicData
    {
        public uint instanceNo;
        public ulong serialNo;
        public ushort level;
        public List<double> buffValue;
    }

    [Serializable]
    public class RelicStoreData
    {
        public ulong serialNo;
        public uint count;
    }

    [Serializable]
    public class BaseCampData
    {
        public ulong serialNo;
        public ushort level;
    }

    [Serializable]
    public class SkillItemData
    {
        public byte type;
        public uint count;
    }

    [Serializable]
    public class BoxData
    {
        public byte type;
        public uint count;
    }

    [Serializable]
    public class BuffValueData
    {
        public byte type;
        public double value;
    }

}