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
            MAX_GROUP
        };
        
        public class UnitRatioData
        {
            public float Ratio;
            public ulong SummonSerialNo;
        }

        static Dictionary<GROUP_ID, List<UnitRatioData>> _unitRatioDataDic = new Dictionary<GROUP_ID, List<UnitRatioData>>();
        static List<int> _cacheList = new List<int>();
        static float[] _unitRatioTotalArray = new float[(int)GROUP_ID.MAX_GROUP];

        static int[] _groupMaxCount = new int[(int)GROUP_ID.MAX_GROUP]
        {
            4, // NORMAL_GROUP
            1 // LEGEND_GROUP
        };

        static Dictionary<string, DataTableListBase> _dataTableDic = new Dictionary<string, DataTableListBase>();

        static List<byte> _captianLIst = new List<byte>();

        public static bool LoadAllDataTable()
        {
            try
            {
                AddDataTable(BossDataTable_List.NAME, new BossDataTable_List());
                AddDataTable(BuffDataTable_List.NAME, new BuffDataTable_List());
                AddDataTable(CaptianDataTable_List.NAME, new CaptianDataTable_List());
                AddDataTable(EnemyDataTable_List.NAME, new EnemyDataTable_List());
                AddDataTable(EnhancementDataTable_List.NAME, new EnhancementDataTable_List());
                AddDataTable(GlobalSettingDataTable_List.NAME, new GlobalSettingDataTable_List());
                AddDataTable(ItemDataTable_List.NAME, new ItemDataTable_List());
                AddDataTable(LevelUpDataTable_List.NAME, new LevelUpDataTable_List());
                AddDataTable(ProjectileDataTable_List.NAME, new ProjectileDataTable_List());
                AddDataTable(StageDataTable_List.NAME, new StageDataTable_List());
                AddDataTable(UnitDataTable_List.NAME, new UnitDataTable_List());
                AddDataTable(UnitSlotDataTable_List.NAME, new UnitSlotDataTable_List());
                AddDataTable(UnitSummonDataTable_List.NAME, new UnitSummonDataTable_List());
                AddDataTable(WaveDataTable_List.NAME, new WaveDataTable_List());
                AddDataTable(WorldDataTable_List.NAME, new WorldDataTable_List());
                AddDataTable(ShopDataTable_List.NAME, new ShopDataTable_List());

                BuildUnitSummonRatioList();
                BuildCaptianList();

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
        }

        public static List<ulong> GetCanBuyUnitList()
        {
            List<ulong> canBuyUnitList = new List<ulong>();
            for (int i = 0; i < (int)GROUP_ID.MAX_GROUP; ++i)
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
    }
}
