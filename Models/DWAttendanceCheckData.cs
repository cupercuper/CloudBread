using System;
using System.Collections.Generic;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWAttendanceCheckDataInputParam
    {
        public string memberID;
        public string token;
    }

    [Serializable]
    public class DWAttendanceCheckDataModel
    {
        public byte attendanceCheck;
        public byte continueAttendanceCnt;
        public byte accAttendanceCnt;
        public short continueAttendanceNo;
        public short accAttendanceNo;
        public long ether;
        public long gas;
        public long gem;
        public double gold;
        public List<SkillItemData> skillItemList;
        public List<BoxData> boxList;
        public byte errorCode;
    }
}