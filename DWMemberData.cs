using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using DW.CommonData;

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

        public static List<ulong> ConvertUnitList(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            int count = 0;
            count = br.ReadInt32();

            List<ulong> unitList = new List<ulong>(count);

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

            bw.Write(mailData.msg);

            bw.Write(mailData.itemData.Count);
            for (int i = 0; i < mailData.itemData.Count; ++i)
            {
                bw.Write(mailData.itemData[i].itemNo);
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

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            mailData.msg = br.ReadString();
            int count = br.ReadInt32();

            for (int i = 0; i < count; ++i)
            {
                DWItemData itemData = new DWItemData();
                itemData.itemNo = br.ReadUInt64();
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

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);
            
            eventData.msg = br.ReadString();

            int count = br.ReadInt32();

            eventData.itemData = new List<DWItemData>();

            for(int i = 0; i <count; ++i)
            {
                DWItemData itemData = new DWItemData();
                itemData.itemNo = br.ReadUInt64();
                itemData.count = br.ReadInt32();

                eventData.itemData.Add(itemData);
            }

            return eventData;
        }

        public static List<long> ConvertEventList(byte[] buffer)
        {
            List<long> eventList = new List<long>();

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            int count = br.ReadInt32();

            for (int i = 0; i < count; ++i)
            {
                eventList.Add(br.ReadInt64());
            }

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

        public static double GetPoint(short worldNo, int changeCaptianCnt)
        {
            double point = worldNo + changeCaptianCnt * 1000;
            return point;
        }

    }
}