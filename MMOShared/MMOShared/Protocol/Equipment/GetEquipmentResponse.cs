using System;
using System.Collections.Generic;

namespace Protocol
{
    [Serializable]
    public class GetEquipmentResponse
    {
        public int ErrorCode;
        public string Message;

        public int CharacterId;

        // 当前装备栏
        public List<EquipmentItemInfo> EquipmentList;

        // 返回当前最终角色属性，方便客户端直接刷新属性面板
        public CharacterInfo CharacterInfo;
    }
}