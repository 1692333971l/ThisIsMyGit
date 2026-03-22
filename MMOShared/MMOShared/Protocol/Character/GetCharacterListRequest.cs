using System;

//获取角色一览请求
namespace Protocol
{
    [Serializable]
    public class GetCharacterListRequest
    {
        // 当前登录用户ID
        public int UserId;
    }
}