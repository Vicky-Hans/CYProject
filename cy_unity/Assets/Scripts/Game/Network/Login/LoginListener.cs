using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.NativeCore;
using DHFramework;
using DHFramework.Json;

namespace DH.Game.Login
{
    public class LoginListener: IULoginListener
    {
        private UniTaskCompletionSource<(ULoginResult result, string dhToken)> loginTcs;
        private UniTaskCompletionSource<(ULoginResult result, string message)> logoutTcs;

        public async UniTask<(ULoginResult result, string dhToken)> Login(LoginType loginType)
        {
            if (loginTcs != null)
            {
                return (ULoginResult.Failed, "Doing login");
            }

            loginTcs = new UniTaskCompletionSource<(ULoginResult result, string dhToken)>();
            ULogin.Login(loginType);
            var result = await loginTcs.Task;
            loginTcs = null;
            return result;
        }

        public async UniTask<(ULoginResult result, string message)> Logout()
        {
            if (logoutTcs != null)
            {
                return (ULoginResult.Failed, "Doing logout");
            }

            logoutTcs = new UniTaskCompletionSource<(ULoginResult result, string dhToken)>();
            ULogin.Logout();
            var result = await logoutTcs.Task;
            logoutTcs = null;
            return result;
        }
        
        void IULoginListener.LoginComplete(LoginType loginType, ULoginResult result, string info)
        {
            DHLog.Debug($"[Login]LoginComplete result:{result}\n info:{info}");

            if (result == ULoginResult.Success)
            {
                // 取出DH_Token用作登录
                // try to deserialize login info json data
                Dictionary<string, object> dic;
                try
                {
                    dic = DHUtility.Json.ToObject<Dictionary<string, object>>(info);
                    if (dic == null)
                    {
                        return;
                    }

                    // check dh token
                    if (!dic.ContainsKey("dh_token"))
                    {
                        DHLog.Error($"[Login]Not have dh_token");
                        return;
                    }

                    string dhToken = dic.ReadValue<string>("dh_token");
                    loginTcs?.TrySetResult((result, dhToken));
                }
                catch (Exception e)
                {
                    DHLog.Error($"[Login]{e}");
                    loginTcs?.TrySetResult((result, string.Empty));
                }
            }
            else
            {
                loginTcs?.TrySetResult((result, string.Empty));
            }
        }

        void IULoginListener.LogoutComplete(LoginType loginType, ULoginResult result, string msg)
        {
            logoutTcs?.TrySetResult((result, msg));
        }

        void IULoginListener.BindComplete(ULoginResult result, string msg)
        {
            
        }
    }
}