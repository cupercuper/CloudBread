using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using CloudBread.globals;
using System.Data.SqlClient;
using System.Data;
using DW.CommonData;

namespace CloudBread
{
    public class DWDataTableManager
    {
        const int CONTINUE_ATTENDANCE_TABLE_CNT = 3;
        const int ACC_ATTENDANCE_TABLE_CNT = 3;

        public enum GROUP_ID
        {
            NORMAL_GROUP = 0,
            LEGEND_GROUP,
            TICKET_1_GROUP,
            TICKET_2_GROUP,
            TICKET_3_GROUP,
            TICKET_4_GROUP,
            TICKET_5_GROUP,
            MAX_GROUP
        };
        
        public class UnitRatioData
        {
            public float Ratio;
            public ulong SummonSerialNo;
        }

        public class LuckySupplyShipData
        {
            public float Ratio;
            public byte Index;
        }

        public class RelicRatioData
        {
            public double Ratio;
            public ulong SerialNo;
            public RelicDataTable RelicDataTable;
        }

        public class MineralBoxData
        {
            public double Value;
            public double DefaultHP;
        }

        public class ScienceDroneData
        {
            public ulong SerialNo;
            public float Ratio;
        }

        public class BoxData
        {
            public float Rate;
            public ulong SerialNo;
        }


        static Dictionary<GROUP_ID, List<UnitRatioData>> _unitRatioDataDic = new Dictionary<GROUP_ID, List<UnitRatioData>>();
        static Dictionary<GROUP_ID, List<UnitRatioData>> _firstUserUnitRatioDataDic = new Dictionary<GROUP_ID, List<UnitRatioData>>();

        static List<int> _cacheList = new List<int>();
        static float[] _unitRatioTotalArray = new float[(int)GROUP_ID.MAX_GROUP];
        static float[] _firstUnitRatioTotalArray = new float[(int)GROUP_ID.MAX_GROUP];

        static Dictionary<string, ShopDataTable> _productIDShopList = new Dictionary<string, ShopDataTable>();
        static Dictionary<string, ulong> _productIDSerialList = new Dictionary<string, ulong>();

        static int[] _groupMaxCount = new int[(int)GROUP_ID.MAX_GROUP]
        {
            4, // NORMAL_GROUP
            1, // LEGEND_GROUP
            0,
            0,
            0,
            0,
            0
        };

        static Dictionary<string, DataTableListBase> _dataTableDic = new Dictionary<string, DataTableListBase>();

        static List<byte> _captianLIst = new List<byte>();

        public static GlobalSettingDataTable GlobalSettingDataTable;

        static List<ScienceDroneData> _scienceDroneDataList = new List<ScienceDroneData>();

        static List<DWItemData>[] _continueAttendanceRewardList = new List<DWItemData>[CONTINUE_ATTENDANCE_TABLE_CNT];
        static List<DWItemData>[] _accAttendanceRewardList = new List<DWItemData>[ACC_ATTENDANCE_TABLE_CNT];
        static Dictionary<byte, Dictionary<byte,List<ulong> > > _dailyQuestList = new Dictionary<byte, Dictionary<byte, List<ulong>>>();
        static List<ulong> _dailyQuestAddRewardList = new List<ulong>();
        static List<byte>[] _dailyQuestTypeList = new List<byte>[(int)DW.CommonData.DAILY_QUEST_GRADE_TYPE.GRADE_5];
        static List<ulong> _firstAchievementList = new List<ulong>();

        static Dictionary<ulong, List<LuckySupplyShipData>> _luckySupplyShipDataList = new Dictionary<ulong, List<LuckySupplyShipData>>();
        static Dictionary<ulong, List<LuckySupplyShipData>> _againLuckySupplyShipDataList = new Dictionary<ulong, List<LuckySupplyShipData>>();

        static Dictionary<MINERAL_BOX_TYPE, MineralBoxData> _mineralBoxList = new Dictionary<MINERAL_BOX_TYPE, MineralBoxData>();

        static List<ulong> _firstUnitList = new List<ulong>();

        static List<RelicRatioData> _relicRatioList = new List<RelicRatioData>();

        static List<SkillItemDataTable> _firstSkillItemList = new List<SkillItemDataTable>();

        static Dictionary<BOX_TYPE, List<BoxData>> _boxDataList = new Dictionary<BOX_TYPE, List<BoxData>>();

        public static bool LoadAllDataTable()
        {
            try
            {
                AddDataTable(AccAttendanceRewardDataTable_List.NAME, new AccAttendanceRewardDataTable_List());
                AddDataTable(AchievementDataTable_List.NAME, new AchievementDataTable_List());
                AddDataTable(ActiveItemDataTable_List.NAME, new ActiveItemDataTable_List());
                AddDataTable(BaseCampDataTable_List.NAME, new BaseCampDataTable_List());
                AddDataTable(BossDataTable_List.NAME, new BossDataTable_List());
                AddDataTable(BossDungeonDataTable_List.NAME, new BossDungeonDataTable_List());
                AddDataTable(BuffDataTable_List.NAME, new BuffDataTable_List());
                AddDataTable(CaptianDataTable_List.NAME, new CaptianDataTable_List());
                AddDataTable(ContinueAttendanceRewardDataTable_List.NAME, new ContinueAttendanceRewardDataTable_List());
                AddDataTable(DailyQuestDataTable_List.NAME, new DailyQuestDataTable_List());
                AddDataTable(EnemyDataTable_List.NAME, new EnemyDataTable_List());
                AddDataTable(EnhancementDataTable_List.NAME, new EnhancementDataTable_List());
                AddDataTable(GlobalSettingDataTable_List.NAME, new GlobalSettingDataTable_List());
                AddDataTable(ItemDataTable_List.NAME, new ItemDataTable_List());
                AddDataTable(LevelUpDataTable_List.NAME, new LevelUpDataTable_List());
                AddDataTable(LevelUpSkillDataTable_List.NAME, new LevelUpSkillDataTable_List());
                AddDataTable(LuckySupplyShipDataTable_List.NAME, new LuckySupplyShipDataTable_List());
                AddDataTable(ModeDataTable_List.NAME, new ModeDataTable_List());
                AddDataTable(ProjectileDataTable_List.NAME, new ProjectileDataTable_List());
                AddDataTable(RelicDataTable_List.NAME, new RelicDataTable_List());
                AddDataTable(RelicDestroyDataTable_List.NAME, new RelicDestroyDataTable_List());
                AddDataTable(RelicSlotDataTable_List.NAME, new RelicSlotDataTable_List());
                AddDataTable(RelicUpgradeDataTable_List.NAME, new RelicUpgradeDataTable_List());
                AddDataTable(ResourceDrillDataTable_List.NAME, new ResourceDrillDataTable_List());
                AddDataTable(RTDModeDataTable_List.NAME, new RTDModeDataTable_List());
                AddDataTable(RTDModeRoundDataTable_List.NAME, new RTDModeRoundDataTable_List());
                AddDataTable(ShopDataTable_List.NAME, new ShopDataTable_List());
                AddDataTable(StageDataTable_List.NAME, new StageDataTable_List());
                AddDataTable(ToolTipDataTable_List.NAME, new ToolTipDataTable_List());
                AddDataTable(TutorialDataTable_List.NAME, new TutorialDataTable_List());
                AddDataTable(TypeOfTypeDataTable_List.NAME, new TypeOfTypeDataTable_List());
                AddDataTable(UnitDataTable_List.NAME, new UnitDataTable_List());
                AddDataTable(UnitSlotDataTable_List.NAME, new UnitSlotDataTable_List());
                AddDataTable(UnitSummonDataTable_List.NAME, new UnitSummonDataTable_List());
                AddDataTable(UnitSummonRandomTicketDataTable_List.NAME, new UnitSummonRandomTicketDataTable_List());
                AddDataTable(UpgradeDataTable_List.NAME, new UpgradeDataTable_List());
                AddDataTable(WaveDataTable_List.NAME, new WaveDataTable_List());
                AddDataTable(WorldDataTable_List.NAME, new WorldDataTable_List());
                AddDataTable(SkillItemDataTable_List.NAME, new SkillItemDataTable_List());
                AddDataTable(MineralBoxDataTable_List.NAME, new MineralBoxDataTable_List());
                AddDataTable(ScienceDroneDataTable_List.NAME, new ScienceDroneDataTable_List());
                AddDataTable(WarpDataTable_List.NAME, new WarpDataTable_List());
                AddDataTable(BoxDataTable_List.NAME, new BoxDataTable_List());
                AddDataTable(RelicInventorySlotDataTable_List.NAME, new RelicInventorySlotDataTable_List());

                //BuildUnitSummonRatioList();
                BuildCaptianList();
                BuildShop();
                BuildScienceDroneNoList();
                BuildAttendanceReward();
                BuildDailyQuestReward();
                BuildFirstAchievement();
                BuildLuckySupplyShip();
                BuildFirstUnitList();
                BuildRelicRatioList();
                BuildSkillItemList();
                BuildMineralBoxList();
                BuildBoxDataList();

                GlobalSettingDataTable = DWDataTableManager.GetDataTable(GlobalSettingDataTable_List.NAME, 1) as GlobalSettingDataTable;
                GlobalSettingDataTable.DailyQuestResetTIme = 13 * 60;
                return true;
            }
            catch(Exception)
            {
                throw;
            }
        }

        static void AddDataTable(string tableName, DataTableListBase dataTableListBase)
        {
            LoadDataTable(tableName, dataTableListBase);
            LoadDataVersion(tableName, dataTableListBase);
            _dataTableDic.Add(tableName, dataTableListBase);
        }

        //static void AddDataTable(string tableName, DataTableListBase dataTableListBase, string fileName)
        //{
        //    _dataTableDic.Add(tableName, dataTableListBase);
        //    LoadDataFile(tableName, fileName);
        //}

        //static void LoadDataFile(string name, string fileName)
        //{
        //    DataTableListBase dataTableListBase = null;
        //    if (!_dataTableDic.TryGetValue(name, out dataTableListBase))
        //    {
        //        return;
        //    }

        //    string curDir = Environment.CurrentDirectory;
        //    string [] splitFileNames = fileName.Split('.');
        //    string convertFileName = splitFileNames[0] + ".bytes";
        //    string dataPath = "./" + convertFileName;// "D:\\StarHeroes\\DataTable\\" + convertFileName;
        //    if(!File.Exists(dataPath))
        //    {
        //        return;
        //    }

        //    using (BinaryReader reader = new BinaryReader(File.Open(dataPath, FileMode.Open)))
        //    {
        //        dataTableListBase.Load(reader);
        //    }
        //}


        static bool LoadDataTable(string tableName, DataTableListBase dataTableListBase)
        {
            SqlConnection conn = new SqlConnection(globalVal.DBConnectionString);

            conn.Open();

            string strQuery = string.Format("SELECT * FROM [{0}DataTable]", tableName);

            SqlCommand command = new SqlCommand(strQuery, conn);

            DataTable dt = new DataTable();
            using (SqlDataAdapter da = new SqlDataAdapter(command))
            {
                da.Fill(dt);
            }

            dataTableListBase.Load(dt);

            return true;
        }

        static bool LoadDataVersion(string tableName, DataTableListBase dataTableListBase)
        {
            SqlConnection conn = new SqlConnection(globalVal.DBConnectionString);

            conn.Open();

            string strQuery = string.Format("SELECT [Version] FROM [DataVersionTable] WHERE TableName = '{0}'", tableName);

            SqlCommand command = new SqlCommand(strQuery, conn);

            DataTable dt = new DataTable();
            using (SqlDataAdapter da = new SqlDataAdapter(command))
            {
                da.Fill(dt);
            }

            foreach (DataRow dr in dt.Rows)
            {
                UInt16 version = UInt16.Parse(dr[0].ToString());
                dataTableListBase.Version = version;
            }

            return true;
        }

        public static DataTableBase GetDataTable(string name, ulong serialNo)
        {
            DataTableListBase dataTableList = null;
            if (!_dataTableDic.TryGetValue(name, out dataTableList))
            {
                return null;
            }

            DataTableBase dataTable = null;
            if (!dataTableList.DataList.TryGetValue(serialNo, out dataTable))
            {
                return null;
            }

            return dataTable;
        }

        public static Dictionary<ulong, DataTableBase> GetDataTableList(string name)
        {
            DataTableListBase dataTableList = null;
            if (!_dataTableDic.TryGetValue(name, out dataTableList))
            {
                return null;
            }

            return dataTableList.DataList;
        }

        //static void BuildUnitSummonRatioList()
        //{
        //    Dictionary<ulong, DataTableBase>  unitSummomnList = GetDataTableList(UnitSummonDataTable_List.NAME);
        //    foreach(KeyValuePair<ulong, DataTableBase> kv in unitSummomnList)
        //    {
        //        UnitSummonDataTable unitSummonDataTable = kv.Value as UnitSummonDataTable;
        //        if(unitSummonDataTable.GroupID >= (int)GROUP_ID.MAX_GROUP)
        //        {
        //            continue;
        //        }

        //        GROUP_ID groupID = (GROUP_ID)unitSummonDataTable.GroupID;
        //        List<UnitRatioData> unitRatioList = null;
        //        if(_unitRatioDataDic.TryGetValue((GROUP_ID)groupID, out unitRatioList) == false)
        //        {
        //            unitRatioList = new List<UnitRatioData>();
        //            _unitRatioDataDic.Add(groupID, unitRatioList);
        //        }

        //        UnitRatioData unitRatioData = new UnitRatioData();
        //        unitRatioData.SummonSerialNo = kv.Key;
        //        unitRatioData.Ratio = unitSummonDataTable.Ratio / 1000.0f;
        //        unitRatioList.Add(unitRatioData);

        //        _unitRatioTotalArray[(int)groupID] += unitRatioData.Ratio;
        //    }

        //    foreach(KeyValuePair<GROUP_ID, List<UnitRatioData>> kv in _unitRatioDataDic)
        //    {
        //        kv.Value.Sort(delegate (UnitRatioData a, UnitRatioData b)
        //        {
        //            if (a.Ratio > b.Ratio)
        //                return -1;
        //            else if (a.Ratio > b.Ratio)
        //                return 1;
        //            else
        //                return 0;
        //        }
        //        );
        //    }

        //    foreach (KeyValuePair<ulong, DataTableBase> kv in unitSummomnList)
        //    {
        //        UnitSummonDataTable unitSummonDataTable = kv.Value as UnitSummonDataTable;
        //        if (unitSummonDataTable.GroupID > (int)GROUP_ID.LEGEND_GROUP)
        //        {
        //            continue;
        //        }

        //        UnitDataTable unitDataTable = DWDataTableManager.GetDataTable(UnitDataTable_List.NAME, unitSummonDataTable.ChangeSerialNo) as UnitDataTable;
        //        if (unitDataTable == null)
        //        {
        //            continue;
        //        }

        //        GROUP_ID groupID = (GROUP_ID)unitSummonDataTable.GroupID;
        //        List<UnitRatioData> unitRatioList = null;
        //        if (_firstUserUnitRatioDataDic.TryGetValue((GROUP_ID)groupID, out unitRatioList) == false)
        //        {
        //            unitRatioList = new List<UnitRatioData>();
        //            _firstUserUnitRatioDataDic.Add(groupID, unitRatioList);
        //        }

        //        UnitRatioData unitRatioData = new UnitRatioData();
        //        unitRatioData.SummonSerialNo = kv.Key;
        //        unitRatioData.Ratio = unitSummonDataTable.Ratio / 1000.0f;
        //        unitRatioList.Add(unitRatioData);

        //        _firstUnitRatioTotalArray[(int)groupID] += unitRatioData.Ratio;
        //    }

        //    foreach (KeyValuePair<GROUP_ID, List<UnitRatioData>> kv in _firstUserUnitRatioDataDic)
        //    {
        //        kv.Value.Sort(delegate (UnitRatioData a, UnitRatioData b)
        //        {
        //            if (a.Ratio > b.Ratio)
        //                return -1;
        //            else if (a.Ratio > b.Ratio)
        //                return 1;
        //            else
        //                return 0;
        //        }
        //        );
        //    }

        //}

        public static List<ulong> GetCanBuyUnitList()
        {
            List<ulong> canBuyUnitList = new List<ulong>();
            for (int i = 0; i < (int)GROUP_ID.LEGEND_GROUP + 1; ++i)
            {
                GetCanBuyUnitList((GROUP_ID)i, ref canBuyUnitList);
            }

            return canBuyUnitList;
        }

        static void GetCanBuyUnitList(GROUP_ID groupID, ref List<ulong> unitLIst)
        {
            List<ulong> summonUnitList = new List<ulong>();

            List<UnitRatioData> unitRatioList = null;
            if(_unitRatioDataDic.TryGetValue(groupID, out unitRatioList) == false)
            {
                return;
            }

            float maxRatio = _unitRatioTotalArray[(int)groupID];
            List<int> inputList = new List<int>();

            Random random = new Random((int)DateTime.Now.Ticks);

            while (true)
            {
                if (summonUnitList.Count == _groupMaxCount[(int)groupID])
                {
                    break;
                }

                float ratio = (float)random.NextDouble() * maxRatio;

                for(int i = 0; i < unitRatioList.Count; ++i)
                {
                    if(inputList.Contains(i))
                    {
                        continue;
                    }

                    if(ratio <= unitRatioList[i].Ratio)
                    {
                        summonUnitList.Add(unitRatioList[i].SummonSerialNo);
                        maxRatio -= unitRatioList[i].Ratio;
                        inputList.Add(i);
                        break;
                    }
                    ratio -= unitRatioList[i].Ratio;
                }
            }

            unitLIst.AddRange(summonUnitList);
        }

        public static List<ulong> GetCanBuyFirstUserUnitList()
        {
            List<ulong> canBuyUnitList = new List<ulong>();
            for (int i = 0; i < (int)GROUP_ID.LEGEND_GROUP + 1; ++i)
            {
                GetCanBuyFirstUserUnitList((GROUP_ID)i, ref canBuyUnitList);
            }

            return canBuyUnitList;
        }

        static void GetCanBuyFirstUserUnitList(GROUP_ID groupID, ref List<ulong> unitLIst)
        {
            List<ulong> summonUnitList = new List<ulong>();

            List<UnitRatioData> unitRatioList = null;
            if (_firstUserUnitRatioDataDic.TryGetValue(groupID, out unitRatioList) == false)
            {
                return;
            }

            float maxRatio = _firstUnitRatioTotalArray[(int)groupID];
            List<int> inputList = new List<int>();

            Random random = new Random((int)DateTime.Now.Ticks);

            while (true)
            {
                if (summonUnitList.Count == _groupMaxCount[(int)groupID])
                {
                    break;
                }

                float ratio = (float)random.NextDouble() * maxRatio;

                for (int i = 0; i < unitRatioList.Count; ++i)
                {
                    if (inputList.Contains(i))
                    {
                        continue;
                    }

                    if (ratio <= unitRatioList[i].Ratio)
                    {
                        summonUnitList.Add(unitRatioList[i].SummonSerialNo);
                        maxRatio -= unitRatioList[i].Ratio;
                        inputList.Add(i);
                        break;
                    }
                    ratio -= unitRatioList[i].Ratio;
                }
            }

            unitLIst.AddRange(summonUnitList);
        }

        public static ulong GetUnitTicket(GROUP_ID groupID)
        {
            List<UnitRatioData> unitRatioList = null;
            if (_unitRatioDataDic.TryGetValue(groupID, out unitRatioList) == false)
            {
                return 0;
            }

            float maxRatio = _unitRatioTotalArray[(int)groupID];

            Random random = new Random((int)DateTime.Now.Ticks);

            float ratio = (float)random.NextDouble() * maxRatio;

            for (int i = 0; i < unitRatioList.Count; ++i)
            {
                if (ratio <= unitRatioList[i].Ratio)
                {
                    return unitRatioList[i].SummonSerialNo;       
                }
                ratio -= unitRatioList[i].Ratio;
            }

            return 0;
        }

        static void BuildCaptianList()
        {
            Dictionary<ulong, DataTableBase> captianLIst = GetDataTableList(CaptianDataTable_List.NAME);
            foreach(KeyValuePair<ulong, DataTableBase> kv in captianLIst)
            {
                CaptianDataTable captainDataTable = kv.Value as CaptianDataTable;
                _captianLIst.Add(captainDataTable.Type);
            }
        }

        public static byte GetCaptianID()
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            int index = random.Next(0, _captianLIst.Count);
            return _captianLIst[index];
        }

        static void BuildShop()
        {
            Dictionary<ulong, DataTableBase> shopLIst = GetDataTableList(ShopDataTable_List.NAME);
            foreach (KeyValuePair<ulong, DataTableBase> kv in shopLIst)
            {
                ShopDataTable shopDataTable = kv.Value as ShopDataTable;
                if(shopDataTable.ProductId == "")
                {
                    continue;
                }

                _productIDShopList.Add(shopDataTable.ProductId, shopDataTable);
                _productIDSerialList.Add(shopDataTable.ProductId, kv.Key);
            }
        }

        public static ShopDataTable GetShopTable(string productID)
        {
            ShopDataTable shopDataTable = null;
            if(_productIDShopList.TryGetValue(productID, out shopDataTable) == false)
            {
                return null;
            }

            return shopDataTable;
        }

        public static ulong GetShopSerialNo(string productID)
        {
            ulong serialNo = 0;
            if (_productIDSerialList.TryGetValue(productID, out serialNo) == false)
            {
                return 0;
            }

            return serialNo;
        }


        static void BuildScienceDroneNoList()
        {
            int maxRatio = 0;
            Dictionary<ulong, DataTableBase> shopLIst = GetDataTableList(ScienceDroneDataTable_List.NAME);
            foreach (KeyValuePair<ulong, DataTableBase> kv in shopLIst)
            {
                ScienceDroneDataTable dataTable = kv.Value as ScienceDroneDataTable;
                if(dataTable == null)
                {
                    continue;
                }

                maxRatio += dataTable.Ratio;
            }

            foreach (KeyValuePair<ulong, DataTableBase> kv in shopLIst)
            {
                ScienceDroneDataTable dataTable = kv.Value as ScienceDroneDataTable;
                if (dataTable == null)
                {
                    continue;
                }

                ScienceDroneData data = new ScienceDroneData();
                data.Ratio = (float)dataTable.Ratio / (float)maxRatio;
                data.SerialNo = kv.Key;

                _scienceDroneDataList.Add(data);
            }

        }

        public static ulong GetScienceDroneNo()
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            float num = (float)random.NextDouble();
            for(int i = 0; i < _scienceDroneDataList.Count; ++i)
            {
                ScienceDroneData data =_scienceDroneDataList[i];
                num -= data.Ratio;
                if(num <= 0.0f)
                {
                    return data.SerialNo;
                }
            }
            return _scienceDroneDataList[_scienceDroneDataList.Count - 1].SerialNo;
        }

        static void BuildAttendanceReward()
        {
            for (int i = 0; i < _continueAttendanceRewardList.Length; ++i)
            {
                _continueAttendanceRewardList[i] = new List<DWItemData>();
            }

            Dictionary<ulong, DataTableBase> continueAttendanceRewardList = GetDataTableList(ContinueAttendanceRewardDataTable_List.NAME);
            foreach (KeyValuePair<ulong, DataTableBase> kv in continueAttendanceRewardList)
            {
                ContinueAttendanceRewardDataTable continueAttendanceRewardDataTable = kv.Value as ContinueAttendanceRewardDataTable;

                DWItemData attendanceRewardData1 = new DWItemData();
                attendanceRewardData1.itemType = continueAttendanceRewardDataTable.Table_ItemType_1;
                attendanceRewardData1.subType = continueAttendanceRewardDataTable.Table_ItemSubType_1;
                attendanceRewardData1.value = continueAttendanceRewardDataTable.Table_ItemValue_1;
                _continueAttendanceRewardList[0].Add(attendanceRewardData1);

                DWItemData attendanceRewardData2 = new DWItemData();
                attendanceRewardData2.itemType = continueAttendanceRewardDataTable.Table_ItemType_2;
                attendanceRewardData2.subType = continueAttendanceRewardDataTable.Table_ItemSubType_2;
                attendanceRewardData2.value = continueAttendanceRewardDataTable.Table_ItemValue_2;
                _continueAttendanceRewardList[1].Add(attendanceRewardData2);

                DWItemData attendanceRewardData3 = new DWItemData();
                attendanceRewardData3.itemType = continueAttendanceRewardDataTable.Table_ItemType_3;
                attendanceRewardData3.subType = continueAttendanceRewardDataTable.Table_ItemSubType_3;
                attendanceRewardData3.value = continueAttendanceRewardDataTable.Table_ItemValue_3;
                _continueAttendanceRewardList[2].Add(attendanceRewardData3);
            }

            for (int i = 0; i < _accAttendanceRewardList.Length; ++i)
            {
                _accAttendanceRewardList[i] = new List<DWItemData>();
            }

            Dictionary<ulong, DataTableBase> accAttendanceRewardList = GetDataTableList(AccAttendanceRewardDataTable_List.NAME);
            foreach (KeyValuePair<ulong, DataTableBase> kv in accAttendanceRewardList)
            {
                AccAttendanceRewardDataTable accAttendanceRewardDataTable = kv.Value as AccAttendanceRewardDataTable;

                DWItemData attendanceRewardData1 = new DWItemData();
                attendanceRewardData1.itemType = accAttendanceRewardDataTable.Table_ItemType_1;
                attendanceRewardData1.subType = accAttendanceRewardDataTable.Table_ItemSubType_1;
                attendanceRewardData1.value = accAttendanceRewardDataTable.Table_ItemValue_1;
                _accAttendanceRewardList[0].Add(attendanceRewardData1);

                DWItemData attendanceRewardData2 = new DWItemData();
                attendanceRewardData2.itemType = accAttendanceRewardDataTable.Table_ItemType_2;
                attendanceRewardData2.subType = accAttendanceRewardDataTable.Table_ItemSubType_2;
                attendanceRewardData2.value = accAttendanceRewardDataTable.Table_ItemValue_2;
                _accAttendanceRewardList[1].Add(attendanceRewardData2);

                DWItemData attendanceRewardData3 = new DWItemData();
                attendanceRewardData3.itemType = accAttendanceRewardDataTable.Table_ItemType_3;
                attendanceRewardData3.subType = accAttendanceRewardDataTable.Table_ItemSubType_3;
                attendanceRewardData3.value = accAttendanceRewardDataTable.Table_ItemValue_3;
                _accAttendanceRewardList[2].Add(attendanceRewardData3);
            }
        }

        static void BuildDailyQuestReward()
        {
            Dictionary<ulong, DataTableBase> dailyQuestList = GetDataTableList(DailyQuestDataTable_List.NAME);
            foreach (KeyValuePair<ulong, DataTableBase> kv in dailyQuestList)
            {
                DailyQuestDataTable dailyQuestDataTable = kv.Value as DailyQuestDataTable;

                if(dailyQuestDataTable.Grade == (int)DW.CommonData.DAILY_QUEST_GRADE_TYPE.GRADE_ADD_REWARD)
                {
                    _dailyQuestAddRewardList.Add(kv.Key);
                    continue;
                }

                Dictionary<byte, List<ulong> > questTypeList = null;
                if(_dailyQuestList.TryGetValue(dailyQuestDataTable.Grade, out questTypeList) == false)
                {
                    if (dailyQuestDataTable.Grade != (int)DW.CommonData.DAILY_QUEST_GRADE_TYPE.GRADE_ADD_REWARD)
                    {
                        _dailyQuestTypeList[dailyQuestDataTable.Grade - 1] = new List<byte>();
                    }
                    questTypeList = new Dictionary<byte, List<ulong>>();
                    _dailyQuestList.Add(dailyQuestDataTable.Grade, questTypeList);
                }

                List<ulong> questNoList = null;
                if(questTypeList.TryGetValue(dailyQuestDataTable.QuestType, out questNoList) == false)
                {
                    if (dailyQuestDataTable.Grade != (int)DW.CommonData.DAILY_QUEST_GRADE_TYPE.GRADE_ADD_REWARD)
                    {
                        _dailyQuestTypeList[dailyQuestDataTable.Grade - 1].Add(dailyQuestDataTable.QuestType);
                    }
                    questNoList = new List<ulong>();
                    questTypeList.Add(dailyQuestDataTable.QuestType, questNoList);
                }

                questNoList.Add(kv.Key);
            }
        }

        public static int GetContinueAttendanceTableNo()
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            return random.Next(0, _continueAttendanceRewardList.Length);
        }

        public static List<DWItemData> GetContinueAttendanceTable(int tableNo)
        {
            if(_continueAttendanceRewardList.Length <= tableNo || tableNo < 0)
            {
                return null;
            }


            return _continueAttendanceRewardList[tableNo];
        }

        public static int GetAccAttendanceTableNo()
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            return random.Next(0, _accAttendanceRewardList.Length);
        }

        public static List<DWItemData> GetAccAttendanceTable(int tableNo)
        {
            if (_accAttendanceRewardList.Length <= tableNo || tableNo < 0)
            {
                return null;
            }

            return _accAttendanceRewardList[tableNo];
        }

        public static List<ulong> GetDailyQuestList()
        {
            List<byte> [] typeList = new List<byte>[(int)DW.CommonData.DAILY_QUEST_GRADE_TYPE.GRADE_5];

            for(int i = 0; i < (int)DW.CommonData.DAILY_QUEST_GRADE_TYPE.GRADE_5; ++i)
            {
                typeList[i] = new List<byte>();
                typeList[i].AddRange(_dailyQuestTypeList[i].ToArray());
            }

            List<ulong> dailyQuestList = new List<ulong>();
            Random random = new Random((int)DateTime.Now.Ticks);
            for (int i = 0; i < (int)DW.CommonData.DAILY_QUEST_GRADE_TYPE.GRADE_ADD_REWARD; ++i)
            {
                if(i == (int)DW.CommonData.DAILY_QUEST_GRADE_TYPE.GRADE_ADD_REWARD - 1)
                {
                    dailyQuestList.Add(_dailyQuestAddRewardList[random.Next(0, _dailyQuestAddRewardList.Count)]);
                }
                else
                {
                    Dictionary<byte, List<ulong>> dailyQuestTypeList = null;
                    if(_dailyQuestList.TryGetValue((byte)(i + 1), out dailyQuestTypeList) == false)
                    {
                        continue;
                    }

                    List<ulong> dailyQuestNoList = null;
                    int typeIdx = random.Next(0, typeList[i].Count);
                    byte questType = typeList[i][typeIdx];
                    if (dailyQuestTypeList.TryGetValue(questType, out dailyQuestNoList) == false)
                    {
                        continue;
                    }

                    dailyQuestList.Add(dailyQuestNoList[random.Next(0, dailyQuestNoList.Count)]);

                    // 한번 사용한 Quest Type은 사용 못한다.
                    // 같은 Type의 퀘스트를 안주기 위해서
                    for(int k = i + 1; k < typeList.Length; ++k)
                    {
                        typeList[k].Remove(questType);
                    }
                }
            }

            return dailyQuestList;

        }

        static void BuildFirstAchievement()
        {
            List<ulong> serialNoList = new List<ulong>();
            List<ulong> nextSerialNoList = new List<ulong>();

            Dictionary<ulong, DataTableBase>  dataTableList = GetDataTableList(AchievementDataTable_List.NAME);
            foreach(KeyValuePair<ulong, DataTableBase> kv in dataTableList)
            {
                serialNoList.Add(kv.Key);
                AchievementDataTable achievementDataTable = kv.Value as AchievementDataTable;
                nextSerialNoList.Add(achievementDataTable.NextAchievement);
            }

            for(int i = 0; i < nextSerialNoList.Count; ++i)
            {
                serialNoList.Remove(nextSerialNoList[i]);
            }

            _firstAchievementList.Clear();
            _firstAchievementList.AddRange(serialNoList.ToArray());
        }

        public static List<ulong> FirstAchievementList()
        {
            return _firstAchievementList;
        }

        static void BuildLuckySupplyShip()
        {
            Dictionary<ulong, DataTableBase> dataTableList = GetDataTableList(LuckySupplyShipDataTable_List.NAME);
            foreach(KeyValuePair<ulong, DataTableBase> kv in dataTableList)
            {
                LuckySupplyShipDataTable luckySupplyShipDataTable = kv.Value as LuckySupplyShipDataTable;
                int totalRatio = 0;
                int againTotalRatio = 0;
                for(int i = 0; i < luckySupplyShipDataTable.ItemTypeList.Count; ++i)
                {
                    int ratio = luckySupplyShipDataTable.ItemRateList[i];
                    totalRatio += ratio;

                    if(luckySupplyShipDataTable.ItemTypeList[i] != -1)
                    {
                        againTotalRatio += ratio;
                    }
                }

                List<LuckySupplyShipData> luckySupplyShipDataList = new List<LuckySupplyShipData>();
                List<LuckySupplyShipData> againLuckySupplyShipDataList = new List<LuckySupplyShipData>();
                for (int i = 0; i < luckySupplyShipDataTable.ItemTypeList.Count; ++i)
                {
                    LuckySupplyShipData luckySupplyShipData = new LuckySupplyShipData();
                    luckySupplyShipData.Ratio = (float)luckySupplyShipDataTable.ItemRateList[i] / (float)totalRatio;
                    luckySupplyShipData.Index = (byte)i;
                    luckySupplyShipDataList.Add(luckySupplyShipData);

                    if (luckySupplyShipDataTable.ItemTypeList[i] != -1)
                    {
                        LuckySupplyShipData againLuckySupplyShipData = new LuckySupplyShipData();
                        againLuckySupplyShipData.Ratio = (float)luckySupplyShipDataTable.ItemRateList[i] / (float)againTotalRatio;
                        againLuckySupplyShipData.Index = (byte)i;
                        againLuckySupplyShipDataList.Add(againLuckySupplyShipData);
                    }
                }

                luckySupplyShipDataList.Sort(delegate(LuckySupplyShipData a, LuckySupplyShipData b)
                {
                    if (a.Ratio > b.Ratio) return -1;
                    else if (a.Ratio < b.Ratio) return -1;
                    return 0;
                });

                againLuckySupplyShipDataList.Sort(delegate (LuckySupplyShipData a, LuckySupplyShipData b)
                {
                    if (a.Ratio > b.Ratio) return -1;
                    else if (a.Ratio < b.Ratio) return -1;
                    return 0;
                });

                _luckySupplyShipDataList.Add(kv.Key, luckySupplyShipDataList);
                _againLuckySupplyShipDataList.Add(kv.Key, againLuckySupplyShipDataList);
            }
        }

        public static byte GetLuckySupplyShipItemIdx(ulong serialNo)
        {
            List<LuckySupplyShipData> luckySupplyShipDataList = null;
            if(_luckySupplyShipDataList.TryGetValue(serialNo, out luckySupplyShipDataList) == false)
            {
                return byte.MaxValue;
            }

            Random random = new Random((int)DateTime.Now.Ticks);
            float num = (float)random.NextDouble();
            for(int i = 0; i < luckySupplyShipDataList.Count; ++i)
            {
                num -= luckySupplyShipDataList[i].Ratio;
                if (num <= 0.0f)
                {
                    return luckySupplyShipDataList[i].Index;
                }
            }

            return luckySupplyShipDataList[luckySupplyShipDataList.Count - 1].Index;
        }

        public static byte GetAgainLuckySupplyShipItemIdx(ulong serialNo)
        {
            List<LuckySupplyShipData> luckySupplyShipDataList = null;
            if (_againLuckySupplyShipDataList.TryGetValue(serialNo, out luckySupplyShipDataList) == false)
            {
                return byte.MaxValue;
            }

            Random random = new Random((int)DateTime.Now.Ticks);
            float num = (float)random.NextDouble();
            for (int i = 0; i < luckySupplyShipDataList.Count; ++i)
            {
                num -= luckySupplyShipDataList[i].Ratio;
                if (num <= 0.0f)
                {
                    return luckySupplyShipDataList[i].Index;
                }
            }

            return luckySupplyShipDataList[luckySupplyShipDataList.Count - 1].Index;
        }

        static void BuildFirstUnitList()
        {
            Dictionary<ulong, DataTableBase> unitList = GetDataTableList(UnitDataTable_List.NAME);
            foreach(KeyValuePair<ulong, DataTableBase> kv in unitList)
            {
                UnitDataTable unitDataTable = kv.Value as UnitDataTable;
                if(unitDataTable.OpenStage == 0)
                {
                    _firstUnitList.Add(kv.Key);
                }
            }
        }

        static public List<ulong> GetFirstUnitList()
        {
            return _firstUnitList;
        }

        static void BuildRelicRatioList()
        {
            double ratioMax = 0;
            Dictionary<ulong, DataTableBase> relicList = GetDataTableList(RelicDataTable_List.NAME);
            foreach(KeyValuePair<ulong, DataTableBase> kv in relicList)
            {
                RelicDataTable relicDataTable = kv.Value as RelicDataTable;
                if(relicDataTable == null)
                {
                    continue;
                }

                ratioMax += (double)relicDataTable.SummonRatio;
            }

            foreach (KeyValuePair<ulong, DataTableBase> kv in relicList)
            {
                RelicDataTable relicDataTable = kv.Value as RelicDataTable;
                if (relicDataTable == null)
                {
                    continue;
                }

                RelicRatioData relicRatioData = new RelicRatioData();
                relicRatioData.Ratio = (double)relicDataTable.SummonRatio / ratioMax;
                relicRatioData.SerialNo = kv.Key;
                relicRatioData.RelicDataTable = relicDataTable;
                _relicRatioList.Add(relicRatioData);
            }

            _relicRatioList.Sort(delegate (RelicRatioData a, RelicRatioData b)
            {
                if (a.Ratio < b.Ratio) return -1;
                else if (a.Ratio > b.Ratio) return 1;
                return 0;
            });
        }

        public static ulong GetRelicNo(Dictionary<uint, RelicData> relicDataList)
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            double num = random.NextDouble();
            int idx = 0;
            while (true)
            {
                if(idx >= _relicRatioList.Count)
                {
                    num = random.NextDouble();
                    idx = 0;
                }

                num -= _relicRatioList[idx].Ratio;
                if (num <= 0.0f)
                {
                    bool insert = true;
                    foreach (KeyValuePair<uint, RelicData> kv in relicDataList)
                    {
                        if(kv.Value.serialNo == _relicRatioList[idx].SerialNo)
                        {
                            insert = false;
                            break;
                        }
                    }

                    if(insert)
                    {
                        return _relicRatioList[idx].SerialNo;
                    }
                }

                ++idx;
            }
        }

        static void BuildSkillItemList()
        {
            Dictionary<ulong, DataTableBase> skillItemLIst = GetDataTableList(SkillItemDataTable_List.NAME);
            foreach(KeyValuePair<ulong, DataTableBase> kv in skillItemLIst)
            {
                SkillItemDataTable itemDataTable = kv.Value as SkillItemDataTable;
                if(itemDataTable == null)
                {
                    continue;
                }

                if(itemDataTable.OpenStage == 0)
                {
                    _firstSkillItemList.Add(itemDataTable);
                }
            }
        }

        public static List<SkillItemDataTable> GetFirstSkillItemList()
        {
            return _firstSkillItemList;
        }

        static void BuildMineralBoxList()
        {
            Dictionary<ulong, DataTableBase> mineralBoxLIst = GetDataTableList(MineralBoxDataTable_List.NAME);
            foreach (KeyValuePair<ulong, DataTableBase> kv in mineralBoxLIst)
            {
                MineralBoxDataTable boxDataTable =kv.Value as MineralBoxDataTable;
                if(boxDataTable == null)
                {
                    continue;
                }

                EnemyDataTable enemy = DWDataTableManager.GetDataTable(EnemyDataTable_List.NAME, boxDataTable.DefaultHPNo) as EnemyDataTable;
                if(enemy == null)
                {
                    continue;
                }

                MineralBoxData boxData = new MineralBoxData();
                boxData.Value = (double)boxDataTable.Value / 1000.0;
                boxData.DefaultHP = enemy.HP;

                _mineralBoxList.Add((MINERAL_BOX_TYPE)boxDataTable.BoxType, boxData);
            }
        }

        public static double GetMineral(MINERAL_BOX_TYPE boxType, ulong stageNo)
        {
            MineralBoxData boxData = null;
            if(_mineralBoxList.TryGetValue(boxType, out boxData) == false)
            {
                return 0.0;
            }
            
            double value = boxData.Value;

            return ((double)stageNo / value) * (boxData.DefaultHP * Math.Pow(1.39, Math.Min(stageNo, 115)) * Math.Pow(1.13, Math.Min(stageNo - 115, 0)) * 5.0 * 40.0);
        }

        static void BuildBoxDataList()
        {
            uint[] totalRatio = new uint[(int)BOX_TYPE.MAX_TYPE];
            Dictionary<ulong, DataTableBase> boxList = GetDataTableList(BoxDataTable_List.NAME);
            foreach (KeyValuePair<ulong, DataTableBase> kv in boxList)
            {
                BoxDataTable boxDataTable = kv.Value as BoxDataTable;
                if (boxDataTable == null)
                {
                    continue;
                }

                totalRatio[boxDataTable.BoxType] += boxDataTable.Ratio;
            }

            foreach (KeyValuePair<ulong, DataTableBase> kv in boxList)
            {
                BoxDataTable boxDataTable = kv.Value as BoxDataTable;
                if (boxDataTable == null)
                {
                    continue;
                }

                List<BoxData> boxDataList = null;
                if (_boxDataList.TryGetValue((BOX_TYPE)boxDataTable.BoxType, out boxDataList) == false)
                {
                    boxDataList = new List<BoxData>();
                    _boxDataList.Add((BOX_TYPE)boxDataTable.BoxType, boxDataList);
                }

                BoxData boxData = new BoxData();
                boxData.SerialNo = kv.Key;
                boxData.Rate = (float)boxDataTable.Ratio / (float)totalRatio[boxDataTable.BoxType];

                boxDataList.Add(boxData);
            }
            
        }

        public static List<BoxData> GetBoxDataList(BOX_TYPE boxType)
        {
            List<BoxData> boxDataList = null;
            if (_boxDataList.TryGetValue(boxType, out boxDataList) == false)
            {
                return null;
            }

            return boxDataList;
        }

        public static ulong GetBox(BOX_TYPE boxType)
        {
            List<BoxData> boxDataList = null;
            if (_boxDataList.TryGetValue(boxType, out boxDataList) == false)
            {
                return 0;
            }

            Random rand = new Random((int)DateTime.UtcNow.Ticks);
            float num = (float)rand.NextDouble();
            for(int i = 0; i < boxDataList.Count;++i)
            {
                num -= boxDataList[i].Rate;
                if(num <= 0.0f)
                {
                    return boxDataList[i].SerialNo;
                }
            }

            return boxDataList[boxDataList.Count - 1].SerialNo;
        }

    }
}
