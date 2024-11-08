using System;
using DH.NativeCore;

public class LoginListener : IULoginListener
{
    private Action<LoginType, ULoginResult, string> loginComplete;
    private Action<LoginType, ULoginResult, string> logoutComplete;
    private Action<ULoginResult, string> bindComplete;
    
    public static LoginListener Create(Action<LoginType, ULoginResult, string> loginComplete, 
        Action<LoginType, ULoginResult, string> logoutComplete,
        Action<ULoginResult, string> bindComplete)
    {
        var listener = new LoginListener(loginComplete, logoutComplete, bindComplete);
        return listener;
    }

    public LoginListener(Action<LoginType, ULoginResult, string> loginComplete, Action<LoginType, ULoginResult, string> logoutComplete, Action<ULoginResult, string> bindComplete)
    {
        this.loginComplete = loginComplete;
        this.logoutComplete = logoutComplete;
        this.bindComplete = bindComplete;
    }

    public void LoginComplete(LoginType loginType, ULoginResult result, string info)
    {
        loginComplete?.Invoke(loginType, result, info);
    }

    public void LogoutComplete(LoginType loginType, ULoginResult result, string msg)
    {
        logoutComplete?.Invoke(loginType, result, msg);
    }

    public void BindComplete(ULoginResult result, string msg)
    {
        bindComplete?.Invoke(result, msg);
    }
}