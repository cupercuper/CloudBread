using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWUseSkillItemDataInputParam
    {
        public string memberID;
        public byte itemType;
        public string token;
    }

    [Serializable]
    public class DWUseSkillItemDataModel
    {
        public byte errorCode;
    }
}