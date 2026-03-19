namespace Protocol
{
    public enum ErrorCode
    {
        Success = 0,//成功
        UnknownError = 1,//未知错误
        InvalidParams = 1001,//无效参数
        AccountAlreadyExists = 2001,//账户已存在
        AccountNotFound = 2002,//账户未找到
        PasswordIncorrect = 2003//密码错误
    }
}