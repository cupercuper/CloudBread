using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using DW.CommonData;
using Logger.Logging;

namespace CloudBread
{
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

        public static byte[] ConvertByte(List<UnitData> unitList)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
                
            bw.Write(unitList.Count);
            for(int i = 0; i < unitList.Count; ++i)
            {
                bw.Write(unitList[i].level);
                bw.Write(unitList[i].serialNo);
            }

            bw.Close();
            ms.Close();
            return ms.ToArray();
        }

        public static List<UnitData> ConvertUnitDataList(byte [] buffer)
        {
            List<UnitData> unitList = new List<UnitData>();
            if(buffer == null)
            {
                return unitList;
            }

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            int count = 0;
            count = br.ReadInt32();

            for (int i = 0; i < count; ++i)
            {
                UnitData unitData = new UnitData();
                unitData.level = br.ReadUInt16();
                unitData.serialNo = br.ReadUInt64();

                unitList.Add(unitData);
            }

            br.Close();
            ms.Close();

            return unitList;
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

        //public static List<UnitStoreData> ConvertUnitStoreList(byte[] buffer)
        //{
        //    List<UnitStoreData> unitStoretList = new List<UnitStoreData>();

        //    if (buffer == null)
        //    {
        //        return unitStoretList;
        //    }

        //    MemoryStream ms = new MemoryStream(buffer);
        //    BinaryReader br = new BinaryReader(ms);

        //    int count = br.ReadInt32();

        //    for (int i = 0; i < count; ++i)
        //    {
        //        UnitStoreData unitStoreData = new UnitStoreData();
        //        unitStoreData.serialNo = br.ReadUInt64();
        //        unitStoreData.count = br.ReadInt32();

        //        unitStoretList.Add(unitStoreData);
        //    }
        //    br.Close();
        //    ms.Close();

        //    return unitStoretList;
        //}

        //public static byte[] ConvertByte(List<UnitStoreData> list)
        //{
        //    MemoryStream ms = new MemoryStream();
        //    BinaryWriter bw = new BinaryWriter(ms);

        //    bw.Write(list.Count);
        //    for (int i = 0; i < list.Count; ++i)
        //    {
        //        bw.Write(list[i].serialNo);
        //        bw.Write(list[i].count);
        //    }

        //    bw.Close();
        //    ms.Close();
        //    return ms.ToArray();
        //}

        public static byte[] ConvertByte(List<byte> list)
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

        public static List<byte> ConvertByteList(byte[] buffer)
        {
            List<byte> byteList = new List<byte>();
            if (buffer == null)
            {
                return byteList;
            }

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            int count = 0;
            count = br.ReadInt32();

            for (int i = 0; i < count; ++i)
            {
                byteList.Add(br.ReadByte());
            }

            br.Close();
            ms.Close();
            return byteList;
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

        public static List<uint> ConvertUnitDeckList(byte[] buffer)
        {
            List<uint> unitDeckList = new List<uint>();
            if (buffer == null)
            {
                return unitDeckList;
            }

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            int count = 0;
            count = br.ReadInt32();

            for (int i = 0; i < count; ++i)
            {
                unitDeckList.Add(br.ReadUInt32());
            }

            br.Close();
            ms.Close();
            return unitDeckList;
        }

        public static List<uint> ConvertBossClearList(byte[] buffer)
        {
            List<uint> bossClearList = new List<uint>();
            if (buffer == null)
            {
                return bossClearList;
            }

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            int count = 0;
            count = br.ReadInt32();

            for (int i = 0; i < count; ++i)
            {
                bossClearList.Add(br.ReadUInt32());
            }

            br.Close();
            ms.Close();
            return bossClearList;
        }


        public static byte[] ConvertByte(List<uint> list)
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
                bw.Write(mailData.itemData[i].subType);
                bw.Write(mailData.itemData[i].value);
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
                itemData.subType = br.ReadByte();
                itemData.value = br.ReadString();

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
                itemData.subType = br.ReadByte(); 
                itemData.value = br.ReadString();

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

        public static List<QuestData> ConvertQuestDataList(byte[] buffer)
        {
            List<QuestData> dailyQuestDataList = new List<QuestData>();
            if (buffer == null)
            {
                return dailyQuestDataList;
            }

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            int count = br.ReadInt32();

            for (int i = 0; i < count; ++i)
            {
                QuestData dailyQuestData = new QuestData();
                dailyQuestData.serialNo = br.ReadUInt64();
                dailyQuestData.complete = br.ReadByte();
                dailyQuestData.getReward = br.ReadByte();
                dailyQuestData.curValue = br.ReadString();

                dailyQuestDataList.Add(dailyQuestData);
            }

            br.Close();
            ms.Close();

            return dailyQuestDataList;
        }


        public static byte [] ConvertByte(List<QuestData> list)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(list.Count);
            for (int i = 0; i < list.Count; ++i)
            {
                bw.Write(list[i].serialNo);
                bw.Write(list[i].complete);
                bw.Write(list[i].getReward);
                bw.Write(list[i].curValue);
            }

            bw.Close();
            ms.Close();
            return ms.ToArray();
        }

        public static LuckySupplyShipData ConvertLuckySupplyShipData(byte[] buffer)
        {
            LuckySupplyShipData luckySupplyShipData = new LuckySupplyShipData();
            luckySupplyShipData.itemList = new List<DWItemData>();

            if (buffer == null)
            {
                return luckySupplyShipData;
            }

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            luckySupplyShipData.shipIdx = br.ReadByte();
            luckySupplyShipData.fail = br.ReadByte();

            int count = br.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                DWItemData itemData = new DWItemData();
                itemData.itemType = br.ReadByte();
                itemData.subType = br.ReadByte();
                itemData.value = br.ReadString();

                luckySupplyShipData.itemList.Add(itemData);
            }

            br.Close();
            ms.Close();

            return luckySupplyShipData;
        }

        public static byte[] ConvertByte(LuckySupplyShipData shipData)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(shipData.shipIdx);
            bw.Write(shipData.fail);
            bw.Write(shipData.itemList.Count);
            for (int i = 0; i < shipData.itemList.Count; ++i)
            {
                bw.Write(shipData.itemList[i].itemType);
                bw.Write(shipData.itemList[i].subType);
                bw.Write(shipData.itemList[i].value);
            }

            bw.Close();
            ms.Close();
            return ms.ToArray();
        }

        public static Dictionary<byte, int> ConvertByteDic(byte[] buffer)
        {
            Dictionary<byte, int> byteList = new Dictionary<byte, int>();
            if(buffer == null)
            {
                return byteList;
            }

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            int count = br.ReadInt32();
            for(int i = 0; i < count; ++i)
            {
                byte key = br.ReadByte();
                int value = br.ReadInt32();

                byteList.Add(key, value);
            }

            br.Close();
            ms.Close();

            return byteList;
        }

        public static byte[] ConvertByte(Dictionary<byte, int> byteList)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);


            bw.Write(byteList.Count);
            foreach(KeyValuePair<byte, int> kv in byteList)
            {
                bw.Write(kv.Key);
                bw.Write(kv.Value);
            }

            bw.Close();
            ms.Close();
            return ms.ToArray();
        }

        public static byte[] ConvertByte(double value)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(value);

            bw.Close();
            ms.Close();
            return ms.ToArray();
        }

        public static double ConvertDouble(byte[] buffer)
        {
            double value = 0;
            if (buffer == null)
            {
                return value;
            }

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            value = br.ReadDouble();

            br.Close();
            ms.Close();

            return value;
        }

        public static byte[] ConvertByte(Dictionary<uint, RelicData> dic)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(dic.Count);
            foreach(KeyValuePair<uint, RelicData> kv in dic)
            {
                bw.Write(kv.Key);
                bw.Write(kv.Value.instanceNo);
                bw.Write(kv.Value.serialNo);
                bw.Write(kv.Value.level);
                bw.Write(kv.Value.buffValue.Count);
                for(int i = 0; i < kv.Value.buffValue.Count; ++i)
                {
                    bw.Write(kv.Value.buffValue[i]);
                }
            }

            bw.Close();
            ms.Close();
            return ms.ToArray();
        }

        public static Dictionary<uint, RelicData> ConvertRelicDataDic(byte[] buffer)
        {
            Dictionary<uint, RelicData> relicDataDic = new Dictionary<uint, RelicData>();
            if (buffer == null)
            {
                return relicDataDic;
            }

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            int count = br.ReadInt32();

            for (int i = 0; i < count; ++i)
            {
                uint key = br.ReadUInt32();

                RelicData relicData = new RelicData();
                relicData.instanceNo = br.ReadUInt32();
                relicData.serialNo = br.ReadUInt64();
                relicData.level = br.ReadUInt16();
                relicData.buffValue = new List<double>();
                int buffValueCnt = br.ReadInt32();
                for(int k = 0; k < buffValueCnt; ++k)
                {
                    relicData.buffValue.Add(br.ReadDouble());
                }

                relicDataDic.Add(key, relicData);
            }

            br.Close();
            ms.Close();

            return relicDataDic;
        }

        public static byte[] ConvertByte(Dictionary<ulong, RelicStoreData> dic)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(dic.Count);
            foreach (KeyValuePair<ulong, RelicStoreData> kv in dic)
            {
                bw.Write(kv.Key);
                bw.Write(kv.Value.serialNo);
                bw.Write(kv.Value.count);
            }

            bw.Close();
            ms.Close();
            return ms.ToArray();
        }

        public static Dictionary<ulong, RelicStoreData> ConvertRelicStoreDataDic(byte[] buffer)
        {
            Dictionary<ulong, RelicStoreData> relicDataDic = new Dictionary<ulong, RelicStoreData>();
            if (buffer == null)
            {
                return relicDataDic;
            }

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            int count = br.ReadInt32();

            for (int i = 0; i < count; ++i)
            {
                ulong key = br.ReadUInt64();

                RelicStoreData relicData = new RelicStoreData();
                relicData.serialNo = br.ReadUInt64();
                relicData.count = br.ReadUInt32();

                relicDataDic.Add(key, relicData);
            }

            br.Close();
            ms.Close();

            return relicDataDic;
        }

        public static byte[] ConvertByte(Dictionary<ulong, ushort> dic)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(dic.Count);
            foreach (KeyValuePair<ulong, ushort> kv in dic)
            {
                bw.Write(kv.Key);
                bw.Write(kv.Value);
            }

            bw.Close();
            ms.Close();
            return ms.ToArray();
        }


        public static Dictionary<ulong, ushort> ConvertBaseCampDic(byte[] buffer)
        {
            Dictionary<ulong, ushort> baseCampList = new Dictionary<ulong, ushort>();
            if (buffer == null)
            {
                return baseCampList;
            }

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            int count = br.ReadInt32();

            for (int i = 0; i < count; ++i)
            {
                ulong key = br.ReadUInt64();
                ushort value = br.ReadUInt16();

                baseCampList.Add(key, value);
            }

            br.Close();
            ms.Close();

            return baseCampList;
        }

        public static List<BaseCampData> ConvertBaseCampList(Dictionary<ulong, ushort> dic)
        {
            List<BaseCampData> baseCampList = new List<BaseCampData>();
            if (dic == null)
            {
                return baseCampList;
            }

            foreach(KeyValuePair<ulong, ushort> kv in dic)
            {
                BaseCampData campData = new BaseCampData();
                campData.serialNo = kv.Key;
                campData.level = kv.Value;
                baseCampList.Add(campData);
            }

            return baseCampList;
        }

        public static byte[] ConvertByte(List<SkillItemData> list)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(list.Count);
            for(int i = 0; i < list.Count; ++i)
            {
                bw.Write(list[i].type);
                bw.Write(list[i].count);

            }

            bw.Close();
            ms.Close();
            return ms.ToArray();
        }


        public static List<SkillItemData> ConvertSkillItemList(byte[] buffer)
        {
            List<SkillItemData> skillItemList = new List<SkillItemData>();
            if (buffer == null)
            {
                return skillItemList;
            }

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            int count = br.ReadInt32();

            for (int i = 0; i < count; ++i)
            {
                SkillItemData skillItemData = new SkillItemData();
                skillItemData.type = br.ReadByte();
                skillItemData.count = br.ReadUInt32();

                skillItemList.Add(skillItemData);
            }

            br.Close();
            ms.Close();

            return skillItemList;
        }

        public static byte[] ConvertByte(List<BoxData> list)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(list.Count);
            for (int i = 0; i < list.Count; ++i)
            {
                bw.Write(list[i].type);
                bw.Write(list[i].count);

            }

            bw.Close();
            ms.Close();
            return ms.ToArray();
        }


        public static List<BoxData> ConvertBoxDataList(byte[] buffer)
        {
            List<BoxData> boxDataList = new List<BoxData>();
            if (buffer == null)
            {
                return boxDataList;
            }

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            int count = br.ReadInt32();

            for (int i = 0; i < count; ++i)
            {
                BoxData boxData = new BoxData();
                boxData.type = br.ReadByte();
                boxData.count = br.ReadUInt32();

                boxDataList.Add(boxData);
            }

            br.Close();
            ms.Close();

            return boxDataList;
        }

        public static byte[] ConvertByte(List<BuffValueData> list)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(list.Count);
            for (int i = 0; i < list.Count; ++i)
            {
                bw.Write(list[i].type);
                bw.Write(list[i].value);
            }

            bw.Close();
            ms.Close();
            return ms.ToArray();
        }

        public static List<BuffValueData> ConvertBuffValueList(byte[] buffer)
        {
            List<BuffValueData> buffValueList = new List<BuffValueData>();
            if(buffer == null)
            {
                return buffValueList;
            }

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            int count = br.ReadInt32();

            for (int i = 0; i < count; ++i)
            {
                BuffValueData buffValueData = new BuffValueData();
                buffValueData.type = br.ReadByte();
                buffValueData.value = br.ReadDouble();

                buffValueList.Add(buffValueData);
            }

            br.Close();
            ms.Close();

            return buffValueList;
        }

        public static byte[] ConvertByte(List<DWItemData> itemList)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(itemList.Count);
            for (int i = 0; i < itemList.Count; ++i)
            {
                bw.Write(itemList[i].itemType);
                bw.Write(itemList[i].subType);
                bw.Write(itemList[i].value);
            }

            bw.Close();
            ms.Close();
            return ms.ToArray();
        }

        public static List<DWItemData> ConvertItemList(byte[] buffer)
        {
            List<DWItemData> itemList = new List<DWItemData>();
            if (buffer == null)
            {
                return itemList;
            }

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            int count = br.ReadInt32();

            for (int i = 0; i < count; ++i)
            {
                DWItemData itemData = new DWItemData();
                itemData.itemType = br.ReadByte();
                itemData.subType = br.ReadByte();
                itemData.value = br.ReadString();

                itemList.Add(itemData);
            }

            br.Close();
            ms.Close();

            return itemList;
        }

        public static double GetPoint(short worldNo, long changeCaptianCnt)
        {
            double point = worldNo + changeCaptianCnt * 1000;
            return point;
        }

        public static bool AddEther(ref long ether, ref long cashEther, long addFreeEther, long addCashEther, Logging.CBLoggers logMessage)
        {            
            if(long.MaxValue - ether < addFreeEther)
            {
                ether = long.MaxValue;
            }
            else
            {
                ether += addFreeEther;
            }

            if (long.MaxValue - cashEther < addCashEther)
            {
                cashEther = long.MaxValue;
            }
            else
            {
                cashEther += addCashEther;
            }

            logMessage.Message = string.Format("AddEther ether = {0}, cashEther = {1}, addFreeEther = {2}, addCashEther = {3}", ether, cashEther, addFreeEther, addCashEther);

            return true;
        }

        public static bool SubEther(ref long ether, ref long cashEther, long subEther, Logging.CBLoggers logMessage)
        {
            if(ether + cashEther < subEther)
            {
                return false;
            }

            ether -= subEther;
            
            if (ether < 0)
            {
                cashEther += ether;
                ether = 0;
            }

            logMessage.Message = string.Format("SubEther ether = {0}, cashEther = {1}, subEther = {2}", ether, cashEther, subEther);

            return true;
        }

        public static bool AddGas(ref long gas, ref long cashGas, long addFreeGas, long addCashGas, Logging.CBLoggers logMessage)
        {
            if (long.MaxValue - gas < addFreeGas)
            {
                gas = long.MaxValue;
            }
            else
            {
                gas += addFreeGas;
            }

            if (long.MaxValue - cashGas < addCashGas)
            {
                cashGas = long.MaxValue;
            }
            else
            {
                cashGas += addCashGas;
            }

            logMessage.Message = string.Format("AddGas gas = {0}, cashGas = {1}, addFreeGas = {2}, addCashGas = {3}", gas, cashGas, addFreeGas, addCashGas);

            return true;
        }

        public static bool SubGas(ref long gas, ref long cashGas, long subGas, Logging.CBLoggers logMessage)
        {
            if (gas + cashGas < subGas)
            {
                return false;
            }

            gas -= subGas;

            if (gas < 0)
            {
                cashGas += gas;
                gas = 0;
            }

            logMessage.Message = string.Format("SubEther Gas = {0}, cashGas = {1}, subGas = {2}", gas, cashGas, subGas);

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

        public static bool DailyQuestRefresh(ref DateTime acceptTime, ref List<QuestData> dailyQuestList, ref long remainTime)
        {
            TimeSpan resetTime = new TimeSpan(DWDataTableManager.GlobalSettingDataTable.DailyQuestResetTIme, 0, 0);
            TimeSpan subTime = DateTime.UtcNow - acceptTime;
            if(subTime.TotalHours < (double)DWDataTableManager.GlobalSettingDataTable.DailyQuestResetTIme)
            {
                resetTime = resetTime.Subtract(subTime);
                remainTime = resetTime.Ticks;
                return false;
            }

            acceptTime = DateTime.UtcNow;
            remainTime = resetTime.Ticks;

            dailyQuestList.Clear();

            List<ulong> dailyQuestNoList = DWDataTableManager.GetDailyQuestList();

            for (int i = (int)DAILY_QUEST_GRADE_TYPE.GRADE_1; i < (int)DAILY_QUEST_GRADE_TYPE.MAX_TYPE; ++i)
            {
                QuestData dailyQuestData = new QuestData();
                dailyQuestData.serialNo = dailyQuestNoList[i - 1];;
                dailyQuestData.complete = 0;
                dailyQuestData.getReward = 0;
                dailyQuestData.curValue = "0";

                dailyQuestList.Add(dailyQuestData);
            }

            return true;
        }

        public static void RefreshDrillIdx(DateTime drillStartTime, ref byte drillIdx, ref long remainTime, ref List<DWItemData> itemList)
        {
            remainTime = 0;
            if (drillIdx == 0)
            {
                itemList.Clear();
                return;
            }

            ResourceDrillDataTable drillDataTable = DWDataTableManager.GetDataTable(ResourceDrillDataTable_List.NAME, (ulong)drillIdx) as ResourceDrillDataTable; 
            if(drillDataTable == null)
            {
                return;
            }

            DateTime endTime = drillStartTime.AddHours(drillDataTable.ResetTime);
            if(endTime <= DateTime.UtcNow)
            {
                drillIdx = 0;
                itemList.Clear();
                return;
            }

            TimeSpan subTime = endTime - DateTime.UtcNow;
            remainTime = subTime.Ticks;
        }

        public static void RefreshLuckySupplyShipRemainTime(DateTime lastTime, ref long remainTime)
        {
            remainTime = 0;

            if (lastTime <= DateTime.UtcNow)
            {
                remainTime = 0;
                return;
            }

            TimeSpan subTime = lastTime - DateTime.UtcNow;
            remainTime = subTime.Ticks;
        }

        public static void AddItem(DWItemData itemData, ref double gold, ref long gem, ref long cashGem, ref long ether, ref long cashEther, ref long gas, ref long cashGas, ref long relicBoxCnt, ref List<SkillItemData> skillItemList, ref List<BoxData> boxList, ref bool droneAdvertisingOff, ulong stageNo, Logging.CBLoggers logMessage, bool build = true, bool cash = false)
        {
            switch ((ITEM_TYPE)itemData.itemType)
            {
                case ITEM_TYPE.GOLD_TYPE:
                    {
                        double value = double.Parse(itemData.value);
                        gold += value;
                    }
                    break;
                case ITEM_TYPE.GEM_TYPE:
                    {
                        uint value = uint.Parse(itemData.value);
                        if (cash == false)
                        {
                            if (AddGem(ref gem, ref cashGem, value, 0, logMessage) == false)
                            {

                            }
                        }
                        else
                        {
                            if (AddGem(ref gem, ref cashGem, 0, value, logMessage) == false)
                            {

                            }
                        }
                    }
                    break;
                case ITEM_TYPE.ETHER_TYPE:
                    {
                        uint value = uint.Parse(itemData.value);
                        if (cash == false)
                        {
                            if (AddEther(ref ether, ref cashEther, value, 0, logMessage) == false)
                            {

                            }
                        }
                        else
                        {
                            if (AddEther(ref ether, ref cashEther, 0, value, logMessage) == false)
                            {

                            }
                        }
                    }
                    break;
                case ITEM_TYPE.SKILL_ITEM_TYPE:
                    {
                        if(skillItemList == null)
                        {
                            return;
                        }

                        uint value = uint.Parse(itemData.value);

                        if (build)
                        {
                            List<byte> typeList = new List<byte>();
                            for (int i = 0; i < skillItemList.Count; ++i)
                            {
                                typeList.Add(skillItemList[i].type);
                            }

                            Random rand = new Random((int)DateTime.Now.Ticks);
                            int idx = rand.Next(0, typeList.Count);

                            SkillItemData skillData = skillItemList[idx];
                            if (skillData == null)
                            {
                                skillData = new SkillItemData();
                                skillItemList.Add(skillData);
                            }
                            skillData.count += value;
                            itemData.subType = skillData.type;
                        }
                        else
                        {
                            SkillItemData skillData = skillItemList.Find(a => a.type == itemData.subType);
                            if (skillData == null)
                            {
                                skillData = new SkillItemData();
                                skillItemList.Add(skillData);
                            }
                            skillData.count += value;
                        }
                    }
                    break;
                case ITEM_TYPE.BOX_TYPE:
                    {
                        if (boxList == null)
                        {
                            return;
                        }

                        uint value = uint.Parse(itemData.value);
                        BoxData boxData = boxList.Find(a => a.type == itemData.subType);
                        if (boxData == null)
                        {
                            boxData = new BoxData();
                            boxData.type = itemData.subType;
                            boxList.Add(boxData);
                        }
                        boxData.count += value;
                    }
                    break;
                case ITEM_TYPE.GAS_TYPE:
                    {
                        
                        uint value = uint.Parse(itemData.value);
                        if (cash == false)
                        {
                            if (AddGas(ref gas, ref cashGas, value, 0, logMessage) == false)
                            {

                            }
                        }
                        else
                        {
                            if (AddGas(ref gas, ref cashGas, 0, value, logMessage) == false)
                            {

                            }
                        }
                    }
                    break;
                case ITEM_TYPE.MINERAL_BOX_TYPE:
                    {
                        if (build)
                        {
                            double mineralBox = DWDataTableManager.GetMineral((MINERAL_BOX_TYPE)itemData.subType, stageNo);
                            mineralBox = Math.Truncate(mineralBox);
                            itemData.value = mineralBox.ToString();

                            gold += mineralBox;
                        }
                        else
                        {
                            gold += double.Parse(itemData.value);
                        }
                    }
                    break;
                case ITEM_TYPE.RELIC_BOX_TYPE:
                    {
                        uint value = uint.Parse(itemData.value);
                        relicBoxCnt += value;
                    }
                    break;
                case ITEM_TYPE.DRONE_ADVERTISING_OFF_TYPE:
                    {
                        droneAdvertisingOff = true;
                    }
                    break;
            }
        }

        public static double GetBuffValue(Random random, int min, int max)
        {
            double value = 0.0;
            if (min == max)
            {
                // 버프 값은 소수점 2자리만 쓰지만 데이터 테이블에서는 공통으로 모든 변수를 소수점 세자리까지 쓰게한다.
                int tempValue = min / 10;
                value = (double)tempValue / 100.0;
            }
            else
            {
                // 유물 버프 값은 소수점 2자리만 쓴다 한자리는 날려 버린다.
                int tempMin = min / 10;
                int tempMax = max / 10;
                // 한자리 버린 상태에서 100으로 나눠서 소수점 2자리만 쓰게 한다.
                value = (double)random.Next(tempMin, tempMax) / 100.0;
            }

            return value;
        }

        public static bool InsertRelicInstanceNo(Dictionary<uint, RelicData> relicDataDic, out uint instanceNo)
        {
            if (relicDataDic.Count == 0)
            {
                instanceNo = 1;
            }
            else
            {
                instanceNo = relicDataDic.Keys.Last();
            }
            uint lastIntanceNo = instanceNo;
            while (true)
            {
                instanceNo++;
                if (instanceNo == uint.MaxValue)
                {
                    instanceNo = 1;
                }
                else if(instanceNo == lastIntanceNo)
                {
                    return false;
                }

                RelicData relicData = null;
                if(relicDataDic.TryGetValue(instanceNo, out relicData) == false)
                {
                    return true;
                }
            }
        }

        public static void AddBuffValueDataList(ref List<BuffValueData> buffValueDataList,  ulong buffNo, double originValue, double nextValue)
        {
            BuffDataTable buffDataTable = DWDataTableManager.GetDataTable(BuffDataTable_List.NAME, buffNo) as BuffDataTable;

            if (buffDataTable.BuffType == (byte)BUFF_TYPE.RETURN_ETHER ||
                buffDataTable.BuffType == (byte)BUFF_TYPE.LUCKY_SHIP_MINERAL ||
                buffDataTable.BuffType == (byte)BUFF_TYPE.RETURN_GAS ||
                buffDataTable.BuffType == (byte)BUFF_TYPE.RETURN_STAGE)
            {
                BuffValueData buffValueData = buffValueDataList.Find(a => a.type == buffDataTable.BuffType);
                if (buffValueData == null)
                {
                    buffValueData = new BuffValueData();
                    buffValueData.type = buffDataTable.BuffType;
                    buffValueDataList.Add(buffValueData);
                }
                else
                {
                    buffValueData.value -= originValue;
                }

                buffValueData.value += nextValue;
            }
        }
    }
}