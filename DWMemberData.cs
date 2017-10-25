using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;


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

        public static byte[] ConvertByte(List<ulong> list)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(list.Count);
            // 꺼꾸로 넣어 준다. 나중에 읽을떄 바로 읽기 위해서
            for(int i = list.Count - 1; i >= 0; --i)
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

        public static uint AddUnitDic(ref Dictionary<uint, UnitData> unitDic, ulong serialNo)
        {
            uint[] keys = new uint[unitDic.Keys.Count];
            unitDic.Keys.CopyTo(keys, 0);

            uint lastKey = keys[keys.Length - 1];
            uint curKey = lastKey;
            while (true)
            {
                if (curKey == uint.MaxValue)
                {
                    curKey = 0;
                    continue;
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

    }
}