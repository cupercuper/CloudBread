﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using DW.CommonData;
using Logger.Logging;

namespace CloudBread
{
    public class UnitData
    {
        public ushort Level;
        public ushort EnhancementCount;
        public ulong SerialNo;
    }

    public class DWMemberData
    {
        public static string[] TEST_MEMBER_ID = new string[]
        {
            "cloud",
            "kkkk"
        };

        public static bool IsTestMemberID(string memberID)
        {
            for(int i = 0; i < TEST_MEMBER_ID.Length; ++i)
            {
                if(TEST_MEMBER_ID[i] == memberID)
                {
                    return true;
                }
            }

            return false;
        }

        public static byte[] ConvertByte(Dictionary<uint, UnitData> unitDic)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
                
            bw.Write(unitDic.Count);
            foreach (KeyValuePair<uint, UnitData> kv in unitDic)
            {
                bw.Write(kv.Key);
                bw.Write(kv.Value.Level);
                bw.Write(kv.Value.EnhancementCount);
                bw.Write(kv.Value.SerialNo);
            }

            bw.Close();
            ms.Close();
            return ms.ToArray();
        }

        public static Dictionary<uint, UnitData> ConvertUnitDic(byte [] buffer)
        {
            Dictionary<uint, UnitData> unitDic = new Dictionary<uint, UnitData>();
            if(buffer == null)
            {
                return unitDic;
            }

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            int count = 0;
            count = br.ReadInt32();

            for (int i = 0; i < count; ++i)
            {
                uint key = br.ReadUInt32();

                UnitData unitDaa = new UnitData();
                unitDaa.Level = br.ReadUInt16();
                unitDaa.EnhancementCount = br.ReadUInt16();
                unitDaa.SerialNo = br.ReadUInt64();

                unitDic.Add(key, unitDaa);
            }

            br.Close();
            ms.Close();

            return unitDic;
        }

        public static List<ClientUnitData> ConvertClientUnitData(Dictionary<uint, UnitData> unitDic)
        {
            List<ClientUnitData> clientUnitDataList = new List<ClientUnitData>();

            foreach(KeyValuePair<uint, UnitData> kv in unitDic)
            {
                ClientUnitData unitData = new ClientUnitData();
                unitData.instanceNo = kv.Key;
                unitData.level = kv.Value.Level;
                unitData.enhancementCount = kv.Value.EnhancementCount;
                unitData.serialNo = kv.Value.SerialNo;

                clientUnitDataList.Add(unitData);
            }

            return clientUnitDataList;
        }

        public static byte[] ConvertByte(List<ulong> list)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(list.Count);
            for(int i = 0; i < list.Count; ++i)
            {
                bw.Write(list[i]);
            }

            bw.Close();
            ms.Close();
            return ms.ToArray();
        }

        public static List<UnitStoreData> ConvertUnitStoreList(byte[] buffer)
        {
            List<UnitStoreData> unitStoretList = new List<UnitStoreData>();

            if (buffer == null)
            {
                return unitStoretList;
            }

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            int count = br.ReadInt32();

            for (int i = 0; i < count; ++i)
            {
                UnitStoreData unitStoreData = new UnitStoreData();
                unitStoreData.serialNo = br.ReadUInt64();
                unitStoreData.count = br.ReadInt32();

                unitStoretList.Add(unitStoreData);
            }
            br.Close();
            ms.Close();

            return unitStoretList;
        }

        public static byte[] ConvertByte(List<UnitStoreData> list)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(list.Count);
            for (int i = 0; i < list.Count; ++i)
            {
                bw.Write(list[i].serialNo);
                bw.Write(list[i].count);
            }

            bw.Close();
            ms.Close();
            return ms.ToArray();
        }


        public static List<ulong> ConvertUnitList(byte[] buffer)
        {
            List<ulong> unitList = new List<ulong>();
            if (buffer == null)
            {
                return unitList;
            }
       
            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            int count = 0;
            count = br.ReadInt32();

            for (int i = 0; i < count; ++i)
            {
                unitList.Add(br.ReadUInt64());
            }

            br.Close();
            ms.Close();
            return unitList;
        }

        public static byte[] ConvertByte(DWMailData mailData)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(mailData.title);
            bw.Write(mailData.msg);

            bw.Write(mailData.itemData.Count);
            for (int i = 0; i < mailData.itemData.Count; ++i)
            {
                bw.Write(mailData.itemData[i].itemType);
                bw.Write(mailData.itemData[i].count);
            }

            bw.Close();
            ms.Close();
            return ms.ToArray();
        }

        public static DWMailData ConvertMailData(byte[] buffer)
        {
            DWMailData mailData = new DWMailData();
            mailData.itemData = new List<DWItemData>();
            if(buffer == null)
            {
                return mailData;
            }

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            mailData.title = br.ReadString();
            mailData.msg = br.ReadString();

            int count = br.ReadInt32();

            for (int i = 0; i < count; ++i)
            {
                DWItemData itemData = new DWItemData();
                itemData.itemType = br.ReadByte();
                itemData.count = br.ReadInt32();

                mailData.itemData.Add(itemData);
            }

            br.Close();
            ms.Close();

            return mailData;
        }

        public static EventData ConvertEventData(byte[] buffer)
        {
            EventData eventData = new EventData();
            eventData.itemData = new List<DWItemData>();
            if(buffer == null)
            {
                return eventData;
            }

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            eventData.title = br.ReadString();
            eventData.msg = br.ReadString();

            int count = br.ReadInt32();

            for(int i = 0; i <count; ++i)
            {
                DWItemData itemData = new DWItemData();
                itemData.itemType = br.ReadByte();
                itemData.count = br.ReadInt32();

                eventData.itemData.Add(itemData);
            }

            br.Close();
            ms.Close();

            return eventData;
        }

        public static List<long> ConvertEventList(byte[] buffer)
        {
            List<long> eventList = new List<long>();
            if(buffer == null)
            {
                return eventList;
            }

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            int count = br.ReadInt32();

            for (int i = 0; i < count; ++i)
            {
                eventList.Add(br.ReadInt64());
            }

            br.Close();
            ms.Close();

            return eventList;
        }

        public static byte[] ConvertByte(List<long> list)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(list.Count);
            for (int i = 0; i < list.Count; ++i)
            {
                bw.Write(list[i]);
            }

            bw.Close();
            ms.Close();
            return ms.ToArray();
        }

        public static List<ActiveItemData> ConvertActiveItemList(byte[] buffer)
        {
            List<ActiveItemData> activeItemList = new List<ActiveItemData>();
            if (buffer == null)
            {
                return activeItemList;
            }

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            int count = br.ReadInt32();

            for (int i = 0; i < count; ++i)
            {
                ActiveItemData activeItemData = new ActiveItemData();
                activeItemData.serialNo = br.ReadUInt64();
                activeItemData.limitTime = br.ReadInt32();
                activeItemData.startTime = br.ReadInt64();

                activeItemList.Add(activeItemData);
            }

            br.Close();
            ms.Close();

            return activeItemList;
        }

        public static byte[] ConvertByte(List<ActiveItemData> list)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(list.Count);
            for (int i = 0; i < list.Count; ++i)
            {
                bw.Write(list[i].serialNo);
                bw.Write(list[i].limitTime);
                bw.Write(list[i].startTime);
            }

            bw.Close();
            ms.Close();
            return ms.ToArray();
        }

        public static List<LimitShopItemData> ConvertLimitShopItemDataList(byte[] buffer)
        {
            List<LimitShopItemData> limitShopItemDataList = new List<LimitShopItemData>();
            if (buffer == null)
            {
                return limitShopItemDataList;
            }

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            int count = br.ReadInt32();

            for (int i = 0; i < count; ++i)
            {
                LimitShopItemData limitShopItemData = new LimitShopItemData();
                limitShopItemData.serialNo = br.ReadUInt64();
                limitShopItemData.count = br.ReadByte();

                limitShopItemDataList.Add(limitShopItemData);
            }

            br.Close();
            ms.Close();

            return limitShopItemDataList;
        }

        public static byte[] ConvertByte(List<LimitShopItemData> list)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(list.Count);
            for (int i = 0; i < list.Count; ++i)
            {
                bw.Write(list[i].serialNo);
                bw.Write(list[i].count);
            }

            bw.Close();
            ms.Close();
            return ms.ToArray();
        }

        public static List<DWUnitTicketData> ConvertUnitTicketDataList(byte[] buffer)
        {
            List<DWUnitTicketData> unitTicketDataList = new List<DWUnitTicketData>();
            if (buffer == null)
            {
                return unitTicketDataList;
            }

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            int count = br.ReadInt32();

            for (int i = 0; i < count; ++i)
            {
                DWUnitTicketData unitTicketData = new DWUnitTicketData();
                unitTicketData.ticketType = (UNIT_SUMMON_TICKET_TYPE)br.ReadByte();
                unitTicketData.serialNo = br.ReadUInt64();

                unitTicketDataList.Add(unitTicketData);
            }

            br.Close();
            ms.Close();

            return unitTicketDataList;
        }

        public static byte[] ConvertByte(List<DWUnitTicketData> list)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(list.Count);
            for (int i = 0; i < list.Count; ++i)
            {
                bw.Write((byte)list[i].ticketType);
                bw.Write(list[i].serialNo);
            }

            bw.Close();
            ms.Close();
            return ms.ToArray();
        }

        public static uint AddUnitDic(ref Dictionary<uint, UnitData> unitDic, ulong serialNo)
        {
            if(unitDic.Count == 0)
            {
                UnitData unitData = new UnitData()
                {
                    EnhancementCount = 0,
                    Level = 1,
                    SerialNo = serialNo
                };

                unitDic.Add(1, unitData);

                return 1;
            }

            uint[] keys = new uint[unitDic.Keys.Count];
            unitDic.Keys.CopyTo(keys, 0);

            uint lastKey = keys[keys.Length - 1];
            uint curKey = lastKey;
            while (true)
            {
                if (curKey == uint.MaxValue)
                {
                    curKey = 0;
                }

                curKey++;
                if(curKey == lastKey)
                {
                    break;
                }

                if (unitDic.ContainsKey(curKey) == false)
                {
                    UnitData unitData = new UnitData()
                    {
                        EnhancementCount = 0,
                        Level = 1,
                        SerialNo = serialNo
                    };

                    unitDic.Add(curKey, unitData);
                    return curKey;
                }
            }

            return 0;
        }

        public static double GetPoint(short worldNo, long changeCaptianCnt)
        {
            double point = worldNo + changeCaptianCnt * 1000;
            return point;
        }

        public static bool AddEnhancedStone(ref long enhancedStone,ref long cashEnhancedStone, long addFreeEnhancedStone, long addCashEnhancedStone, Logging.CBLoggers logMessage)
        {            
            if(long.MaxValue - enhancedStone < addFreeEnhancedStone)
            {
                enhancedStone = long.MaxValue;
            }
            else
            {
                enhancedStone += addFreeEnhancedStone;
            }

            if (long.MaxValue - cashEnhancedStone < addCashEnhancedStone)
            {
                cashEnhancedStone = long.MaxValue;
            }
            else
            {
                cashEnhancedStone += addCashEnhancedStone;
            }

            logMessage.Message = string.Format("AddEnhancedStone enhancedStone = {0}, cashEnhancedStone = {1}, addFreeEnhancedStone = {2}, addCashEnhancedStone = {3}", enhancedStone, cashEnhancedStone, addFreeEnhancedStone, addCashEnhancedStone);

            return true;
        }

        public static bool SubEnhancedStone(ref long enhancedStone, ref long cashEnhancedStone, long subEnhancedStone, Logging.CBLoggers logMessage)
        {
            if(enhancedStone + cashEnhancedStone < subEnhancedStone)
            {
                return false;
            }

            enhancedStone -= subEnhancedStone;
            
            if (enhancedStone < 0)
            {
                cashEnhancedStone += enhancedStone;
                enhancedStone = 0;
            }

            logMessage.Message = string.Format("SubEnhancedStone enhancedStone = {0}, cashEnhancedStone = {1}, subEnhancedStone = {2}", enhancedStone, cashEnhancedStone, subEnhancedStone);

            return true;
        }

        public static bool AddGem(ref long gem, ref long cashGem, long addFreeGem, long addCashGem, Logging.CBLoggers logMessage)
        {
            if (long.MaxValue - gem < addFreeGem)
            {
                gem = long.MaxValue;
            }
            else
            {
                gem += addFreeGem;
            }

            if (long.MaxValue - cashGem < addCashGem)
            {
                cashGem = long.MaxValue;
            }
            else
            {
                cashGem += addCashGem;
            }

            logMessage.Message = string.Format("AddGem gem = {0}, cashGem = {1}, addFreeGem = {2}, addCashGem = {3}", gem, cashGem, addFreeGem, addCashGem);

            return true;
        }

        public static bool SubGem(ref long gem, ref long cashGem, long subGem, Logging.CBLoggers logMessage)
        {
            if (gem + cashGem < subGem)
            {
                return false;
            }

            gem -= subGem;

            if (gem < 0)
            {
                cashGem += gem;
                gem = 0;
            }

            logMessage.Message = string.Format("SubGem gem = {0}, cashgem = {1}, subGem = {2}", gem, cashGem, subGem);

            return true;
        }

        public static void UpdateActiveItem(List<ActiveItemData> activeItemList)
        {
            if(activeItemList == null)
            {
                return;
            }

            DateTime utcTime = DateTime.UtcNow;
            for (int i = activeItemList.Count - 1; i >= 0; --i)
            {
                ActiveItemData activeItemData = activeItemList[i];
                if (activeItemData.limitTime < 0)
                {
                    continue;
                }

                DateTime startTIme = new DateTime(activeItemData.startTime);
                TimeSpan subTime = utcTime - startTIme;
                if (subTime.TotalMinutes >= activeItemData.limitTime)
                {
                    activeItemList.RemoveAt(i);
                }
            }
        }

        public static void AddActiveItem(List<ActiveItemData> activeItemList, ulong serialNo)
        {
            ActiveItemDataTable activeItemDataTable = DWDataTableManager.GetDataTable(ActiveItemDataTable_List.NAME, serialNo) as ActiveItemDataTable;

            bool add = true;
            for(int i = 0; i < activeItemList.Count; ++i)
            {
                if(activeItemList[i].serialNo == serialNo)
                {
                    if(activeItemDataTable.Time <= 0)
                    {
                        activeItemList[i].limitTime = -1;
                    }
                    else if(activeItemList[i].limitTime > 0)
                    {
                        activeItemList[i].limitTime += activeItemDataTable.Time;
                    }

                    add = false;
                    break;
                }
            }

            if(add == true)
            {
                ActiveItemData activeItemData = new ActiveItemData();
                activeItemData.serialNo = serialNo;
                activeItemData.startTime = DateTime.UtcNow.Ticks;
                activeItemData.limitTime = activeItemDataTable.Time <= 0 ? -1 : activeItemDataTable.Time;

                activeItemList.Add(activeItemData);
            }
        }

        public static void BossDungeonTicketRefresh(ref DateTime refreshTime, ref int bossDungeonTicket, int timeZone, int ticketMaxCnt)
        {
            bool refresh = false;
            DateTime timeZoneRefreshTime = refreshTime.AddHours(timeZone);
            DateTime currentTime = DateTime.UtcNow.AddHours(timeZone);

            if(timeZoneRefreshTime.Month != currentTime.Month)
            {
                refresh = true;
               
            }
            else if (timeZoneRefreshTime.Day != currentTime.Day)
            {
                refresh = true;
            }

            if (refresh)
            {
                refreshTime = DateTime.UtcNow;
                if (bossDungeonTicket < ticketMaxCnt)
                {
                    bossDungeonTicket = ticketMaxCnt;
                }
            }
        }
    }
}