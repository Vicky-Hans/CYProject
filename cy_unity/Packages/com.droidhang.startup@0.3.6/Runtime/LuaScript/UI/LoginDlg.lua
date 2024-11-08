---
--- 文件名称:  LoginDlg
--- 创建者:    nieshihai
--- 创建时间:  2022/5/10 18:16
-------------------------------------------------------------------
--- 功能描述：
---
---

local BaseDlg = require("Client/UI/BaseDlg")
local LoginData = require("Data/LoginData")
local LoginMgr = require("Manager/LoginMgr")
local NetworkManager = require("Network/NetworkManager")
local LoginType = CS.DH.NativeCore.LoginType

--模块
---@class LoginDlg:BaseDlg
---@field enterForLogout boolean @是通过登出进入的
local LoginDlg = bpcClass("LoginDlg", BaseDlg)

LoginDlg.uiMap = {
    ["serversObj"] = {LuaUT.GameObject, "Content/ServersObj"},
    ["debugLoginObj"] = {LuaUT.GameObject, "Content/DebugLogin"},
    ["guestBtn"] = {LuaUT.Button, "Content/DebugLogin/GuestLoginBtn"},
    ["switchBtn"] = {LuaUT.Button, "Content/DebugLogin/SwitchBtn"},
    ["curAccountText"] = {LuaUT.Text, "Content/DebugLogin/CurAccount/Account"},
    ["newAccountInput"] = {LuaUT.InputField, "Content/DebugLogin/NewAccount"},
    ["selectedServerBtn"] = {LuaUT.Button, "Content/ServersObj/SelectedServerBtn"},
    ["stateIconFree"] = {LuaUT.GameObject, "Content/ServersObj/SelectedServerBtn/StateIcon/Free"},
    ["stateIconWait"] = {LuaUT.GameObject, "Content/ServersObj/SelectedServerBtn/StateIcon/Wait"},
    ["stateIconCrowded"] = {LuaUT.GameObject, "Content/ServersObj/SelectedServerBtn/StateIcon/Crowded"},


    ["serverSidText"] = {LuaUT.Text, "Content/ServersObj/SelectedServerBtn/ServerName"},
    ["loginBtn"] = {LuaUT.Button, "Content/ServersObj/LoginBtn"},
    ["logoutBtn"] = {LuaUT.Button, "Content/ServersObj/LogoutBtn"},
}

function LoginDlg:OnUIReady()
    self.uiMap = self:Bind(self.rootObj, LoginDlg.uiMap)
    self.onLoginAccount = function(sender, propertyName, oldValue, login)
        if not login then
            self.uiMap.serversObj:SetActive(false)
            self.uiMap.debugLoginObj:SetActive(false)
            self:StartLogin()
        else
            self.uiMap.serversObj:SetActive(true)
            self.uiMap.debugLoginObj:SetActive(false)
        end

    end
    
    self.onSelectOneServer = function(sender, propertyName, oldValue, sid)
        self:RefreshSelectedServerInfo(sid)
    end
    
    self.onConnected = function(result)
        self:StartLogin()
    end

    if self.enterForLogout then
        UIManager.ignoreCloseUILayer = false
        UIManager:CloseOtherDlg(self)
        EventSystemManager:EnableEventSystem("Logout")
        self:InitUI()
        LoginMgr.needUpdate = true
    else
        self:InitUI()
    end
end

function LoginDlg:InitUI()
    self:AddBinding()
    self:BindClick()
    self:TryConnectGameLogic()
end

function LoginDlg:BindClick()
    self:SetBtnClick(self.uiMap.guestBtn, function()
        LoginMgr:LoginSdk(LoginType.Guest)
    end)

    self:SetBtnClick(self.uiMap.selectedServerBtn, function()
        UIManager:Open(UIPathConfig.SelectServerDlg)
    end)
    
    self:SetBtnClick(self.uiMap.switchBtn, function()
        local newId = self.uiMap.newAccountInput.text
        self:ChangeShuMeiDroidHangID(newId)
        self:ShowDebugLoginObj()
    end)
    
    self:SetBtnClick(self.uiMap.loginBtn, function()
        LoginMgr:DoLoginGame()
    end)
    
    self:SetBtnClick(self.uiMap.logoutBtn, function()
        self:Logout()
    end)
end

function LoginDlg:AddBinding()
    LoginData.data:Subscribe("loginAcc", self.onLoginAccount)
    LoginData.data:Subscribe("serverId", self.onSelectOneServer)
end

function LoginDlg:RemoveBinding()
    LoginData.data:Unsubscribe("loginAcc", self.onLoginAccount)
    LoginData.data:Unsubscribe("serverId", self.onSelectOneServer)
end

function LoginDlg:RefreshSelectedServerInfo(serverId)
    if serverId and serverId > 0 then
        local serverItem = LoginData:GetSvrInfoBySid(serverId)

        if serverItem then
            local serverInfo = serverItem.serverData
            UIHelper.SetText(self.uiMap.serverSidText,serverInfo.name)
        end
    end
end

function LoginDlg:ShowDebugLoginObj()
    local curAccount = self:GetShuMeiDroidHangID()
    UIHelper.SetText(self.uiMap.curAccountText, curAccount)
    self.uiMap.debugLoginObj:SetActive(true)
    self.uiMap.serversObj:SetActive(false)
end

--- 更改数美id
---@param id string 更改数美id
function LoginDlg:ChangeShuMeiDroidHangID(id)
    PlayerPrefs:SetString("ULogin_Auto_Login_Key", "");

    ---save to local
    PlayerPrefs:SetString("CurrentDevAccount_ID", id);

    ---Editor模式下清除Shumei
    PlayerPrefs:SetString("ShuMeiID", "");
end

--- 获取当前数美id
function LoginDlg:GetShuMeiDroidHangID()
    return PlayerPrefs:GetString("CurrentDevAccount_ID", "")
end

function LoginDlg:TryConnectGameLogic()
    if not NetworkManager:IsConnect() then
        NetworkManager:RegisterConnectEvent(self.onConnected)
        NetworkManager:Connect()
    else
        self:StartLogin()
    end
end

function LoginDlg:StartLogin()
    if DEBUG then
        self:ShowDebugLoginObj()
    else
        LoginMgr:LoginSdk(LoginType.Domestic)
    end
end

function LoginDlg:Logout()
    LoginMgr:Logout(function()
        NetworkManager:Close()
        NetworkManager:UnregisterConnectEvent(self.onConnected)
    end, true)
end

function LoginDlg:OnDestroy()
    LoginDlg.super.OnDestroy(self)
    self:RemoveBinding()
    NetworkManager:UnregisterConnectEvent(self.onConnected)
end

return LoginDlg