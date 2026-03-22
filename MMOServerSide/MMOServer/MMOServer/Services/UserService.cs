using MMOServer.Core;
using MMOServer.Database;
using MMOServer.Models;
using Protocol;
using System;

namespace MMOServer.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;

        public UserService()
        {
            _userRepository = new UserRepository();
        }

        /// <summary>
        /// 处理登录请求
        /// </summary>
        public NetMessage HandleLogin(NetMessage requestMessage)
        {
            LoginRequest request = JsonHelper.FromJson<LoginRequest>(requestMessage.BodyJson);
            LoginResponse response = new LoginResponse();

            Logger.Info($"HandleLogin: Account = {request.Account}");

            if (string.IsNullOrWhiteSpace(request.Account) || string.IsNullOrWhiteSpace(request.Password))
            {
                response.ErrorCode = (int)ErrorCode.InvalidParams;
                response.Message = "账号或密码不能为空";
                return BuildLoginResponse(response);
            }

            try
            {
                UserEntity user = _userRepository.GetByAccount(request.Account);

                if (user == null)
                {
                    response.ErrorCode = (int)ErrorCode.AccountNotFound;
                    response.Message = "账号不存在";
                    return BuildLoginResponse(response);
                }

                if (user.Password != request.Password)
                {
                    response.ErrorCode = (int)ErrorCode.PasswordIncorrect;
                    response.Message = "密码错误";
                    return BuildLoginResponse(response);
                }

                response.ErrorCode = (int)ErrorCode.Success;
                response.Message = "登录成功";
                response.UserId = user.Id;
                response.Account = user.Account;

                return BuildLoginResponse(response);
            }
            catch (Exception ex)
            {
                Logger.Error($"HandleLogin failed: {ex.Message}");

                response.ErrorCode = (int)ErrorCode.UnknownError;
                response.Message = "登录失败，服务器异常";
                return BuildLoginResponse(response);
            }
        }

        /// <summary>
        /// 处理注册请求
        /// </summary>
        public NetMessage HandleRegister(NetMessage requestMessage)
        {
            RegisterRequest request = JsonHelper.FromJson<RegisterRequest>(requestMessage.BodyJson);
            RegisterResponse response = new RegisterResponse();

            Logger.Info($"HandleRegister: Account = {request.Account}");

            if (string.IsNullOrWhiteSpace(request.Account) || string.IsNullOrWhiteSpace(request.Password))
            {
                response.ErrorCode = (int)ErrorCode.InvalidParams;
                response.Message = "账号或密码不能为空";
                return BuildRegisterResponse(response);
            }

            try
            {
                if (_userRepository.Exists(request.Account))
                {
                    response.ErrorCode = (int)ErrorCode.AccountAlreadyExists;
                    response.Message = "账号已存在";
                    return BuildRegisterResponse(response);
                }

                bool success = _userRepository.Insert(request.Account, request.Password);

                if (success)
                {
                    response.ErrorCode = (int)ErrorCode.Success;
                    response.Message = "注册成功";
                }
                else
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "注册失败";
                }

                return BuildRegisterResponse(response);
            }
            catch (Exception ex)
            {
                Logger.Error($"HandleRegister failed: {ex.Message}");

                response.ErrorCode = (int)ErrorCode.UnknownError;
                response.Message = "注册失败，服务器异常";
                return BuildRegisterResponse(response);
            }
        }

        private NetMessage BuildLoginResponse(LoginResponse response)
        {
            return new NetMessage
            {
                MessageId = (int)MessageId.LoginResponse,
                BodyJson = JsonHelper.ToJson(response)
            };
        }

        private NetMessage BuildRegisterResponse(RegisterResponse response)
        {
            return new NetMessage
            {
                MessageId = (int)MessageId.RegisterResponse,
                BodyJson = JsonHelper.ToJson(response)
            };
        }
    }
}