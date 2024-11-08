---
--- 文件名称:  LoginListenerMgr
--- 创建者:    nieshihai
--- 创建时间:  2022/5/7 10:28
-------------------------------------------------------------------
--- 功能描述：
---
---

--模块
---@class LoginListenerMgr
local LoginListenerMgr = bpcClass("LoginListenerMgr")
local USE_ONCE_DHID_FOR_INSTALL = true

function LoginListenerMgr:Init()
    self:SetULoginListener()
end

---@public
function LoginListenerMgr:SetLoginCallback(callback)
    self.LoginCallback = callback;
end

---@public
function LoginListenerMgr:SetLogoutCallback(callback)
    self.LogoutCallback = callback;
end

---@public
function LoginListenerMgr:SetBindCallback(callback)
    self.BindCallback = callback;
end

---@private
function LoginListenerMgr:SetULoginListener()
    local login = CS.DH.NativeCore.ULogin

    login.UseOnceDHIdForInstall(USE_ONCE_DHID_FOR_INSTALL);
    local listener = CS.LoginListener.Create(
            function(loginType, result, info)
                self:LoginComplete(loginType, result, info)
            end,
            function(loginType, result, msg)
                self:LogoutComplete(loginType, result, msg)
            end,
            function(result, msg)
                self:BindComplete(result, msg)
            end
    )

    login.SetULoginListener(listener)
end

--- 登录回调
---@param loginType DH.NativeCore.LoginType
---@param result DH.NativeCore.ULoginResult
---@param info string
---@private
function LoginListenerMgr:LoginComplete(loginType, result, info)
    bpcPrintf("<b>Login complete</b> %s", result)

    if self.LoginCallback then
        self.LoginCallback(loginType, result, info)
    end
end

--- 登出回调
---@param loginType DH.NativeCore.LoginType
---@param result DH.NativeCore.ULoginResult
---@param msg string
---@private
function LoginListenerMgr:LogoutComplete(loginType, result, msg)
    bpcPrintf("ULogin登出成功:\n" .. msg)

    if self.LogoutCallback then
        self.LogoutCallback(loginType, result, msg)
    end
end

--- 绑定回调
---@param result DH.NativeCore.ULoginResult
---@param msg string
---@private
function LoginListenerMgr:BindComplete(result, msg)
    bpcPrintf("ULogin绑定账号成功:\n" .. msg)

    if self.BindCallback then
        self.BindCallback(result, msg)
    end
end

---@private
function LoginListenerMgr:Release()
    self.LoginCallback = nil;
    self.LogoutCallback = nil;
    self.BindCallback = nil;
    
    local login = CS.DH.NativeCore.ULogin
    login.SetULoginListener(nil)
end

return LoginListenerMgr