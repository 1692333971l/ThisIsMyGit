//请求响应id
namespace Protocol
{
    public enum MessageId
    {
        None = 0,

        // 登录请求响应
        LoginRequest = 1000,
        LoginResponse = 1001,
        // 注册请求响应
        RegisterRequest = 1002,
        RegisterResponse = 1003,
        // 角色创建
        CreateCharacterRequest = 1004,
        CreateCharacterResponse = 1005,
        // 获取角色列表
        GetCharacterListRequest = 1006,
        GetCharacterListResponse = 1007,
        // 进入主城
        EnterGameRequest = 1008,
        EnterGameResponse = 1009,
        // 玩家进入通知
        PlayerEnterNotify = 1010,
        // 玩家离开通知
        PlayerLeaveNotify = 1011,
        // 玩家移动请求
        PlayerMoveRequest = 1012,
        // 玩家移动广播
        PlayerMoveNotify = 1013,
        // 玩家退出请求
        PlayerExitRequest = 1014,
        // 获取背包
        GetInventoryRequest = 1015,
        GetInventoryResponse = 1016,
        // 测试添加道具
        AddItemRequest = 1017,
        AddItemResponse = 1018,
        // 使用道具
        UseItemRequest = 1019,
        UseItemResponse = 1020,
    }
}