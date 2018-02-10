using System;
using System.Collections.Generic;

namespace CloudBread.Models
{
    [Serializable]
    public class DWNickNameCheckInputParam
    {
        public string nickName;
        public string token;
    }

    [Serializable]
    public class DWNickNameCheckModel
    {
        public byte checkType;
        public byte errorCode;
    }
}