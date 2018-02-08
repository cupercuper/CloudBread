﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using CloudBread.globals;
using System.Data.SqlClient;
using System.Data;

namespace CloudBread
{
    public class DWDataTableManager
    {
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

        static Dictionary<GROUP_ID, List<UnitRatioData>> _unitRatioDataDic = new Dictionary<GROUP_ID, List<UnitRatioData>>();
        static Dictionary<GROUP_ID, List<UnitRatioData>> _firstUserUnitRatioDataDic = new Dictionary<GROUP_ID, List<UnitRatioData>>();

        static List<int> _cacheList = new List<int>();
        static float[] _unitRatioTotalArray = new float[(int)GROUP_ID.MAX_GROUP];
        static float[] _firstUnitRatioTotalArray = new float[(int)GROUP_ID.MAX_GROUP];

        static Dictionary<string, ShopDataTable> _productIDShopList = new Dictionary<string, ShopDataTable>();

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

        static List<ulong> _gemBoxNoList = new List<ulong>();

        public static bool LoadAllDataTable()
        {
            try
            {
                AddDataTable(ActiveItemDataTable_List.NAME, new ActiveItemDataTable_List());
                AddDataTable(BossDataTable_List.NAME, new BossDataTable_List());
                AddDataTable(BuffDataTable_List.NAME, new BuffDataTable_List());
                AddDataTable(CaptianDataTable_List.NAME, new CaptianDataTable_List());
                AddDataTable(EnemyDataTable_List.NAME, new EnemyDataTable_List());
                AddDataTable(EnhancementDataTable_List.NAME, new EnhancementDataTable_List());
                AddDataTable(GlobalSettingDataTable_List.NAME, new GlobalSettingDataTable_List());
                AddDataTable(ItemDataTable_List.NAME, new ItemDataTable_List());
                AddDataTable(LevelUpDataTable_List.NAME, new LevelUpDataTable_List());
                AddDataTable(ProjectileDataTable_List.NAME, new ProjectileDataTable_List());
                AddDataTable(ShopDataTable_List.NAME, new ShopDataTable_List());
                AddDataTable(StageDataTable_List.NAME, new StageDataTable_List());
                AddDataTable(UnitDataTable_List.NAME, new UnitDataTable_List());
                AddDataTable(UnitSlotDataTable_List.NAME, new UnitSlotDataTable_List());
                AddDataTable(UnitSummonDataTable_List.NAME, new UnitSummonDataTable_List());
                AddDataTable(UnitSummonRandomTicketDataTable_List.NAME, new UnitSummonRandomTicketDataTable_List());
                AddDataTable(WaveDataTable_List.NAME, new WaveDataTable_List());
                AddDataTable(WorldDataTable_List.NAME, new WorldDataTable_List());
                AddDataTable(GemBoxDataTable_List.NAME, new GemBoxDataTable_List());


                BuildUnitSummonRatioList();
                BuildCaptianList();
                BuildShopBuild();
                BuildGemBoxNoList();


                GlobalSettingDataTable = GetDataTable(GlobalSettingDataTable_List.NAME, 1) as GlobalSettingDataTable;

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

        static void BuildUnitSummonRatioList()
        {
            Dictionary<ulong, DataTableBase>  unitSummomnList = GetDataTableList(UnitSummonDataTable_List.NAME);
            foreach(KeyValuePair<ulong, DataTableBase> kv in unitSummomnList)
            {
                UnitSummonDataTable unitSummonDataTable = kv.Value as UnitSummonDataTable;
                if(unitSummonDataTable.GroupID >= (int)GROUP_ID.MAX_GROUP)
                {
                    continue;
                }

                GROUP_ID groupID = (GROUP_ID)unitSummonDataTable.GroupID;
                List<UnitRatioData> unitRatioList = null;
                if(_unitRatioDataDic.TryGetValue((GROUP_ID)groupID, out unitRatioList) == false)
                {
                    unitRatioList = new List<UnitRatioData>();
                    _unitRatioDataDic.Add(groupID, unitRatioList);
                }

                UnitRatioData unitRatioData = new UnitRatioData();
                unitRatioData.SummonSerialNo = kv.Key;
                unitRatioData.Ratio = unitSummonDataTable.Ratio / 1000.0f;
                unitRatioList.Add(unitRatioData);

                _unitRatioTotalArray[(int)groupID] += unitRatioData.Ratio;
            }

            foreach(KeyValuePair<GROUP_ID, List<UnitRatioData>> kv in _unitRatioDataDic)
            {
                kv.Value.Sort(delegate (UnitRatioData a, UnitRatioData b)
                {
                    if (a.Ratio > b.Ratio)
                        return -1;
                    else if (a.Ratio > b.Ratio)
                        return 1;
                    else
                        return 0;
                }
                );
            }

            foreach (KeyValuePair<ulong, DataTableBase> kv in unitSummomnList)
            {
                UnitSummonDataTable unitSummonDataTable = kv.Value as UnitSummonDataTable;
                if (unitSummonDataTable.GroupID > (int)GROUP_ID.LEGEND_GROUP)
                {
                    continue;
                }

                UnitDataTable unitDataTable = DWDataTableManager.GetDataTable(UnitDataTable_List.NAME, unitSummonDataTable.ChangeSerialNo) as UnitDataTable;
                if (unitDataTable == null || unitDataTable.Grade > 4)
                {
                    continue;
                }

                GROUP_ID groupID = (GROUP_ID)unitSummonDataTable.GroupID;
                List<UnitRatioData> unitRatioList = null;
                if (_firstUserUnitRatioDataDic.TryGetValue((GROUP_ID)groupID, out unitRatioList) == false)
                {
                    unitRatioList = new List<UnitRatioData>();
                    _firstUserUnitRatioDataDic.Add(groupID, unitRatioList);
                }

                UnitRatioData unitRatioData = new UnitRatioData();
                unitRatioData.SummonSerialNo = kv.Key;
                unitRatioData.Ratio = unitSummonDataTable.Ratio / 1000.0f;
                unitRatioList.Add(unitRatioData);

                _firstUnitRatioTotalArray[(int)groupID] += unitRatioData.Ratio;
            }

            foreach (KeyValuePair<GROUP_ID, List<UnitRatioData>> kv in _firstUserUnitRatioDataDic)
            {
                kv.Value.Sort(delegate (UnitRatioData a, UnitRatioData b)
                {
                    if (a.Ratio > b.Ratio)
                        return -1;
                    else if (a.Ratio > b.Ratio)
                        return 1;
                    else
                        return 0;
                }
                );
            }

        }

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

        public static void BuildShopBuild()
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

        static void BuildGemBoxNoList()
        {
            Dictionary<ulong, DataTableBase> shopLIst = GetDataTableList(GemBoxDataTable_List.NAME);
            foreach (KeyValuePair<ulong, DataTableBase> kv in shopLIst)
            {
                _gemBoxNoList.Add(kv.Key);
            }
        }

        public static ulong GetGemBoxNo()
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            return _gemBoxNoList[random.Next(0, _gemBoxNoList.Count)];
        }
    }
}
