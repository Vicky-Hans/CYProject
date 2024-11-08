---
--- 文件名称:  LoginMgr
--- 创建者:    nieshihai
--- 创建时间:  2022/5/5 18:30
-------------------------------------------------------------------
--- 功能描述：
--- 登录sdk=》登录帐号=》请求服务器列表=》登录选择的服务器=》无角色自动创建角色；有角色就登录角色
--- 登出sdk

local NetworkManager = require("Network/NetworkManager")
local LoginListenerMgr = require("Listener/LoginListenerMgr")
local LocalSaveData = require("Data/LocalSaveData")
local LoginData = require("Data/LoginData")
local ULogin = CS.DH.NativeCore.ULogin
local CSULoginResult = CS.DH.NativeCore.ULoginResult
local EventSystemManager = require("Common/EventSystemManager")
local EventModel = "Login"
local UNetErrorCode = CS.DH.UNet.UNetErrorCode

local UGateErrorCode  = {
    ErrOK   = 0,
    ErrFail = -1,

    ErrArgs         = -1003, -- 参数错误
    ErrBusy         = -1005, -- 服务繁忙
    ErrInner        = -1006, -- 内部错误
    ErrRequestParse = -1007, -- 解析请求错误

    ErrDhTokenInvalid        = -206000, -- 登陆token非法
    ErrDhTokenExpired        = -206001, -- 登陆token过期
    ErrServerMaintain        = -206002, -- 服务器维护
    ErrCreateRoleDupName     = -206010, -- 创建角色重名
    ErrCreateRoleInvalidName = -206011, -- 创建角色名字非法
    ErrCreateRoleHawkeye     = -206012, -- 创建角色鹰眼拦截
    ErrLoginRoleLoginLogic   = -206020, -- 角色登陆逻辑服失败
    ErrLoginRoleHawkeye      = -206021, -- 登陆角色鹰眼拦截
    ErrSendFail              = -206030, -- 发送消息失败
    ErrServerInfoNotExist    = -206040, -- 区服信息不存在
    ErrServerAlreadyReg      = -206041, -- 区服已注册
}

local CustomErrorCode = {
    ServerListFailed = 8001,
    UnknowServerState = 8003
}

--模块
---@class LoginMgr
local LoginMgr = bpcClass("LoginMgr")

function LoginMgr:Init()
    LoginData:Init()
    
    --- 服务器错误码回调
    self.errorMessageCallback = function(errorCode)
        EventSystemManager:EnableEventSystem(EventModel)
        BpcDebug.Debug("[LoginMgr]NetError, errorCode -> %s",errorCode)

        if errorCode >= 20000 then
            local MessageBoxManager = require("Manager/MessageBoxManager")
            MessageBoxManager:ResetMessageBoxArgs()
            local title = "Network Error"
            local desc = "Network has disconnect, please reLogin"
            
            --- 收到被服务器踢出消息的处理
            if errorCode == UNetErrorCode.ServerKickNtfClient then
                title = "Tip"
                desc = "You has been kicked out"
            end

            local MsgArgs = MessageBoxManager.MessageBoxArgs
            local forceCloseFunc = function()
                if ProcedureManager:IsCurrent(ProcedureConfig.LoginGameProcedure) then
                    EventSystemManager:EnableEventSystem(EventModel)
                    NetworkManager:Close()
                    NetworkManager:Connect()
                else
                    self:Logout(function()
                        UIManager.ignoreCloseUILayer = true
                        NetworkManager:Close()
                        ProcedureManager:Change(ProcedureConfig.LoginGameProcedure)
                    end, false)
                end

                MessageBoxManager:UnLoadPop()
            end

            --- 确认或背景返回都会走登出流程
            MsgArgs.YesCallback = forceCloseFunc
            MsgArgs.BgCloseCallback = forceCloseFunc
            MsgArgs.Title = title
            MsgArgs.Description = desc
            MessageBoxManager:Open(MsgArgs)
        end
    end
    
    NetworkManager:RegisterErrorMessageEvent(self.errorMessageCallback)
end

---@public
function LoginMgr:Release()
    NetworkManager:UnregisterErrorMessageEvent(self.errorMessageCallback)
end

---@public
function LoginMgr:SetLoginAccountCallback(callback)
    self.LoginAccountCallback = callback;
end

---@public
--- 登录sdk
---@param loginType CS.DH.NativeCore.LoginType @只有两个参数，CSLoginType.Guest|CSLoginType.Domestic
function LoginMgr:LoginSdk(loginType)
    self:Login(function(curLoginType, loginResult, info)
        bpcPrintf("<b>Login complete</b> %s", loginResult)

        ---@type DH.NativeCore.ULoginResult
        if (self:LoginResult(loginResult, info)) then
            self.loginType = loginType
            ---@type LoginInfo
            local loginInfo = Json.decode(info)
            self.loginInfo = loginInfo

            self:AccountAuth()
        end
    end, loginType)
end

---@public
function LoginMgr:CheckUpdateForLogin()
    EventSystemManager:EnableEventSystem(EventModel)
    local state = CS.DH.HotService.HotUpdateManager.Instance.State

    --- 热更新组件正在进行热更新，不需要进行分服热更新检查
    if state ~= CS.DH.HotService.DownloadState.Complete then
        self:OnLoginSuccess()
        return
    end
    
    CS.DH.Launch.StartupEntry.Instance.TaskEntry:CheckUpdateForLogin(LoginData.data.serverId, function()
        CS.DH.Launch.StartupEntry.Instance:HotUpdateForSid(LoginData.data.serverId, function()
            ProcedureManager:Shutdown()
        end)
    end, Handler(self, self.OnLoginSuccess))
end

---@public
function LoginMgr:DoLoginGame()
    EventSystemManager:DisableEventSystem(EventModel)
    
    if LoginData:HasSvrRole() then
        self:LoginRole()
    else
        self:CreateRole()
    end
end

---@private
function LoginMgr:Login(callback, loginType)
    LoginListenerMgr:SetLoginCallback(callback)
    ULogin.Login(loginType)
end

--- 登出sdk
function LoginMgr:Logout(callback, sdkLogout)
    if sdkLogout then
        LoginListenerMgr:SetLogoutCallback(function(loginType, result, msg)
            if (self:LoginResult(result, msg)) then
                LoginData:Release()
            end
            
            if callback then
                callback()
            end
        end)
        
        ULogin.Logout()
    else
        LoginData:Release()
        if callback then
            callback(CS.DH.NativeCore.ULoginResult.Success, nil)
        end
    end
end

---@private
--- 登录账户（由登录流程调用）
function LoginMgr:AccountAuth()
    ---@type reqAccountAuth
    local loginReq = { 
        client = self:CreateClientInfo(),
        token = self.loginInfo.dh_token
    }
    
    bpcPrintf(Json.encode(loginReq))

    NetworkManager:Send(ProtoConfig.msg_1_1, loginReq, function(data)
        ---@type rspAccountAuth
        local reply = data
        bpcPrintf("error %s;error message %s,account id %s", reply.errno, reply.errmsg, reply.accountid)
        
        if self:UGateErrorCode(reply.errno, reply.errmsg) then
            if self.LoginAccountCallback then
                self.LoginAccountCallback(true)
            end
            
            LocalSaveData:SaveRecordAccount(reply.accountid)
            self:RequestServersAndRolesInfo()
        else
            self:OnAuthFailed()
        end
    end)
end

function LoginMgr:OnAuthFailed()
    if self.LoginAccountCallback then
        self.LoginAccountCallback(false)
    end
    
    LocalSaveData:DelRecordAccount()
end

---@private
function LoginMgr:CreateClientInfo()
    local client = {}
    local DeviceUtility = CS.DH.NativeCore.Platform.DeviceUtility
    local Usdk = CS.DH.Usdk
    local bundleid = Usdk.CallFunction("Utils_getUsdkId", "")
    client.dhid = DeviceUtility.GetShuMeiDroidHangID()
    client.bundle_id = bundleid
    client.adid = DeviceUtility.GetGoogleAdvertisingID()
    client.idfv = DeviceUtility.GetIDFV()
    client.imei = DeviceUtility.GetIMEI()
    client.android_id = DeviceUtility.GetAndroidId()
    client.appsflyer_id = DeviceUtility.GetAppflyerID()
    client.device_token = DeviceUtility.GetGoogleDeviceToken()
    client.mac_address = DeviceUtility.GetMacAddress()
    client.device_model = DeviceUtility.GetDeviceModel()
    client.dvice_name = DeviceUtility.GetDeviceName()
    client.os_version = DeviceUtility.GetOS()
    client.language = DeviceUtility.GetLanguage()
    client.network_type = tostring(DeviceUtility.GetNetworkType())
    client.reserved_2 = DeviceUtility.GetOAID()
    client.country = DeviceUtility.GetCountry()
    client.app_version = CS.UnityEngine.Application.version
    client.platform = DeviceUtility.GetPlatform()
    client.channel = "UnityDev"
    client.att = Usdk.CallFunction("Utils_checkATTOpened", "")
    client.sub_package = Usdk.CallFunction(Usdk.Utility.GetTouTiaoId,"{\"plugins\":[\"event_toutiao\"]}")
    return client
end

---@private
--- 请求服务器信息和角色信息
function LoginMgr:RequestServersAndRolesInfo()
    ---@type reqServersAndRoles
    local req = {}
    NetworkManager:Send(ProtoConfig.msg_2_3, req, function(replyData)
        ---@type rspServersAndRoles
        local reply = replyData
        
        if self:UGateErrorCode(reply.errno, reply.errmsg) then
            local servers = reply.servers
            if #servers < 1 then
                self:ErrorLog(3, CustomErrorCode.ServerListFailed, "找不到服务器")
                return
            end
            
            LoginData:ParseServersAndRolesInfo(reply.roles, reply.servers)
        end
    end)
end

---@private
--- 创建角色
function LoginMgr:CreateRole(roleName)
    --TODO 需要项目组复写
    ---@type reqCreateRole
    local reqCreateRole = { logo = 0, name = roleName, sid = LoginData.data.serverId }
    NetworkManager:Send(ProtoConfig.msg_2_1, reqCreateRole, function(replyData)
        ---@type rspCreateRole
        local reply = replyData
        if self:UGateErrorCode(reply.errno, reply.errmsg) then
            local serverInfo = LoginData:GetSvrInfoBySid(LoginData.data.serverId)
            serverInfo.roleData = {roleId = reply.roleId}
            
            self:LoginRole()
        end
    end)
end

---@private
--- 登录角色
function LoginMgr:LoginRole()
    local serverInfo = LoginData:GetSvrInfoBySid(LoginData.data.serverId)
    local role = serverInfo.roleData
    local server = serverInfo.serverData
    
    ---@type reqLoginRole
    local reqLoginRole = { roleId = role.roleId, logicid = server.sid }
    NetworkManager:Send(ProtoConfig.msg_2_2, reqLoginRole, function(replyData)
        ---@type rspLoginRole
        local reply = replyData
        LoginData:SetLoginGameSuccess(reply.errno == 0)
        LocalSaveData:SaveRecordServerId(LoginData.data.serverId)
        
        if self:UGateErrorCode(reply.errno, reply.errmsg) then
            bpcPrintf("Role login success with reply %s", Json.encode(reply))

            Timer:Delay(function()
                self:CheckUpdateForLogin()
            end, 0.1)
        end
    end)
end

function LoginMgr:OnLoginSuccess()
    local serverInfo = LoginData:GetSvrInfoBySid(LoginData.data.serverId)
    local role = serverInfo.roleData
    local server = serverInfo.serverData
    
    self:RoleEnterGameForLua(role, server)
    bpcPrintf("登录成功，开始跳转场景")
    ProcedureManager:Change(ProcedureConfig.MainGameProcedure)
end

function LoginMgr:OnAppNeedUpdate()
    EventSystemManager:EnableEventSystem(EventModel)
    local message = "大版本更新，需要前往商店下载!"
    local vc = CS.Controller.PopupController.Create()
    CS.ViewManager.ScreenManager.Instance:PushMenu("PopWindow",vc)
end

--- 处理ugate错误码
---@return boolean 成功还是失败
---@param errno number errno
---@param errmsg string 错误信息
function LoginMgr:UGateErrorCode(errno, errmsg)
    errmsg = errmsg or ""
    local switch = {
        [UGateErrorCode.ErrOK] = function()
            return true
        end,
        
        --[UGateErrorCode.ErrLoginRoleHawkeye] = function()
        --    local jsonLuaMsg = Json.decode(errmsg)
        --
        --    UIManager:LoadPage(UIConst.login_error_ban_w, false, nil, jsonLuaMsg)
        --    return false
        --end
    }

    if (switch[errno]) then
        return switch[errno]()
    else
        self:ErrorLog(2, errno, errmsg)
        return false
    end
end

---@param loginResult DH.NativeCore.ULoginResult
---@return boolean 是否成功
---@param info string 信息
function LoginMgr:LoginResult(loginResult, info)
    info = info or ""
    if (loginResult == CSULoginResult.Success) then
        bpcPrintTraceback("ULogin登录成功:\n" .. info)
        return true
    elseif (loginResult == CSULoginResult.Cancel) then
        self:ErrorLog(1, loginResult, "ULogin登录取消:" .. info)
        return false
    elseif (loginResult == CSULoginResult.Failed) then
        self:ErrorLog(1, loginResult, "ULogin登录失败:" .. info)
        return false
    elseif (loginResult == CSULoginResult.NetworkRequestError) then
        self:ErrorLog(1, loginResult, "ULogin网络请求错误:" .. info)
        return false
    elseif (loginResult == CSULoginResult.BindInvalidThirdType) then
        self:ErrorLog(1, loginResult, "ULogin绑定无效的第三种类型:" .. info)
        return false
    elseif (loginResult == CSULoginResult.ServerResponseError) then
        self:ErrorLog(1, loginResult, "ULogin服务器响应错误:" .. info)
        return false
    else
        self:ErrorLog(1, loginResult, "ULogin错误码:" .. tostring(loginResult) .. "\n" .. info)
        return false
    end
end

--- 错误打印
function LoginMgr:ErrorLog(errorType, errorCode, errorInfo)
    EventSystemManager:EnableEventSystem(EventModel)
    
    local title = "[登录流程]"
    bpcPrintf(title .. errorInfo)

    local info = ""
    if errorType == 1 then
        info = "ULogin error code: "
    elseif errorType == 2 then
        info = "UGate error code: "
    else
        info = "Custom error code: "
    end

    info = info .. tostring(errorCode)
    self:PopErrorMessageWindow(info)
end

function LoginMgr:PopErrorMessageWindow(errMsg, yesCallback)
    local MessageBoxManager = require("Manager/MessageBoxManager")
    MessageBoxManager:ResetMessageBoxArgs()
    local MsgArgs = MessageBoxManager.MessageBoxArgs
    MsgArgs.YesCallback = function()
        MessageBoxManager:UnLoadPop()
    end
    MsgArgs.Title = "重要提示"
    MsgArgs.Description = errMsg
    MessageBoxManager:Open(MsgArgs)
end

--- 上报sdk角色信息
---@private
---@param role table
---@param server table
function LoginMgr:RoleEnterGameForLua(role, server)
    ---初始化玩家信息
    local info = {
        ---测试数据
        accountId = tostring(role.account),
        sessionId = "",
        sid = tostring(server.sid),
        serverName = server.svr_name,
        userId = tostring(role.roleId),
        userName = role.name,
        level = tostring(role.exp), --TODO
        vip = tostring(role.vexp), --TODO
        avatar = tostring(role.logo),
        token = self.loginInfo.dh_token,
        roleCreateTime = tostring(role.ctime), --tostring(utility.GetGameTime(utility.TicksType.S)),

        email = "boge@163.com",
        roleBalance = "20000",
    }

    CS.DH.Usdk.RoleEnterGameForLua(Json.encode(info))
end

return LoginMgr