---
--- 文件名称:  NetworkManager
--- 创建者:    yuancan.
--- 创建时间:  2021/9/22 2:47 PM
-------------------------------------------------------------------
--- 功能描述：
--- 游戏的主连接管理类
---

require("Common/System")
---@type table<string,ProtoConfigItem>
local pb = require("pb")

--模块
---@class NetworkManager
local NetworkManager = bpcClass("NetworkManager")

local ConnectId = "Game"

local function GetGroupCmdId(group, cmd)
    return group * 1000 + cmd
end


--用于统一提示的唯一错误码
local specErrorCodeConfig = {
}

--忽略，由业务具体负责提示的错误码
local ignoreErrorCodeConfig = {
}

---@private
---@param errorCode number
local function ProcessError(errorCode)
    if errorCode and errorCode < 0 then
        local ignore = ignoreErrorCodeConfig[errorCode]
        if not ignore then
            local specErrorKey = specErrorCodeConfig[errorCode]
            if specErrorKey then
            else
            end
        end
    end
end

local function HandleReceiveMessage(group, cmd, data)
    local id = GetGroupCmdId(group, cmd)
    local idStr = string.format("msg_%s_%s",group,cmd)
    local config = NetworkManager.protoConfig[idStr]

    if config == nil then
        DHLog.Error(string.format("%s 协议找不到", idStr))
    end
    
    local callbackInfo = NetworkManager.mainClient.callback[id]
    local callback = callbackInfo ~= nil and callbackInfo.callback or nil
    local requestData = callbackInfo ~= nil and callbackInfo.data or nil
    local replyMessage = nil
    --- 没有配置pb解析器的情况下，表示消息格式为Json，有应用层解析
    if config.rsp == nil then
        bpcPrintf("没有配置pb解析器，消息格式为Json，请自行处理。")
        replyMessage = data
    else
        replyMessage = pb.decode(config.rsp,data)
    end

    if DEBUG then
        DHLog.Info(string.format("NetManager get cmd, cmd name is %s, cmd data is: %s", config.rsp, DHLog.LuaObj2String(replyMessage)))
    end

    --分发全局注册事件，在主回调之前调用
    NetworkManager:DispatchMsg(group,cmd,replyMessage, requestData, true)

    --- 设置网络数据只读属性
    --- replyMessage = table_read_only(replyMessage)
    if callback then
        callback(replyMessage, requestData)
        NetworkManager.mainClient.callback[id] = nil
    end

    --分发全局注册事件，在主回调之前调用
    NetworkManager:DispatchMsg(group,cmd,replyMessage, requestData, false)
    NetworkManager:ProcessErrorStatus(replyMessage)
end

local function SplitNetAddress(s)
    local p = ":"
    local rt = {}
    string.gsub(
            s,
            "[^" .. p .. "]+",
            function(w)
                table.insert(rt, tostring(w))
            end
    )
    return rt
end

local function SplitGroupCmd(s)
    local p = "_"
    local rt = {}
    string.gsub(
            s,
            "[^" .. p .. "]+",
            function(w)
                table.insert(rt, tonumber(w))
            end
    )
    return rt
end

---@public
function NetworkManager:Init(protoList, protoConfig)
    self.HandleConnectEvent = function()
        self.isConnect = true
        
        if self.isReconnecting then
            --ActivityManager:Hide(ActivityManager.TYPE_NET)
        end
        self.isReconnecting = false
    end

    self.HandleDisConnectEvent = function()
        BpcDebug.Debug("[NetworkManager]收到了网络断开的消息")
        self.isConnect = false

        --ActivityManager:Hide(ActivityManager.TYPE_NET)
    end

    self.HandleReconnectingEvent = function()
        self.isReconnecting = true --标记为重连状态
        BpcDebug.Debug("[NetworkManager]收到了网络断开的消息")
        self.isConnect = false

        --ActivityManager:Show(ActivityManager.TYPE_NET)
    end

    local Manager = CS.NetworkManager.Instance
    Manager:RegisterHandleReceiveMessageEvent(ConnectId, HandleReceiveMessage)
    self:RegisterConnectEvent(self.HandleConnectEvent)
    self:RegisterDisconnectEvent(self.HandleDisConnectEvent)
    self:RegisterReconnectEvent(self.HandleReconnectingEvent)
    self:RegisterErrorMessageEvent(ProcessError)

    local loader = CS.DH.UIFramework.ResourceLoader.Instance
    local textAsset = nil
    for i = 1, #protoList do
        local path = protoList[i]
        loader:LoadAsset(
                path,
                typeof(CS.UnityEngine.TextAsset),
                function(result)
                    textAsset = result
                    assert(pb.load(textAsset.bytes))
                    loader:Unload(result)
                end
        )
    end

    NetworkManager.protoConfig = protoConfig
    --- 初始化Group和Cmd
    for k, v in pairs(protoConfig) do
        local result = SplitGroupCmd(k)
        v.group = result[1]
        v.cmd = result[2]
    end

    self.messageDispatcher = {}
    self.messageDispatcherAfter = {}
    self.handleIndex = 0
    self.isConnect = false
end

---@public
---@param config ProtoConfigItem
---@param requestData table
---@param callback fun(requestData:table):void
function NetworkManager:Send(config, requestData, callback)
    local group = config.group
    local cmd = config.cmd
    self:RegisterCallback(group, cmd, requestData, callback)

    local serializedData = pb.encode(config.req, requestData)
    CS.NetworkManager.Instance:Send(ConnectId, group, cmd, serializedData)
    bpcPrintf("Send msg group %s cmd %s",group,cmd)
    if DEBUG then
        DHLog.Info(string.format("NetManager send cmd, cmd name is %s, cmd data is: %s", config.req, DHLog.LuaObj2String(requestData)))
    end
end

---@public
---@param config ProtoConfigItem
---@param callback fun(requestData:table):void
---@param isBefore boolean @是否需要在处理协议回包前调用
function NetworkManager:AddListener(config, callback, isBefore)
    local id = GetGroupCmdId(config.group, config.cmd)
    local messageList = self.messageDispatcherAfter

    if isBefore then
        messageList = self.messageDispatcher
    end
    
    if not messageList[id] then
        messageList[id] = {}
    end

    self.handleIndex = self.handleIndex + 1
    messageList[id][self.handleIndex] = callback
    return self.handleIndex
end

---@public
---@param config ProtoConfigItem
---@param handle number
function NetworkManager:RemoveListener(config, handle, isBefore)
    local id = GetGroupCmdId(config.group, config.cmd)
    local messageList = self.messageDispatcherAfter

    if isBefore then
        messageList = self.messageDispatcher
    end
    
    if not messageList[id] then
        return
    end
    messageList[id][handle] = nil
end

---@private
---@param group number
---@param cmd number
---@param message table
function NetworkManager:DispatchMsg(group, cmd,message, requestData, isBefore)
    local id = GetGroupCmdId(group, cmd)
    local messageList = self.messageDispatcherAfter

    if isBefore then
        messageList = self.messageDispatcher
    end
    
    local listeners = messageList[id]
    if not listeners then
        return
    end

    for _, v in pairs(listeners) do
        v(message, requestData)
    end
end

---@private
function NetworkManager:RegisterCallback(group, cmd, requestData, callback)
    local id = GetGroupCmdId(group, cmd)
    if NetworkManager.mainClient == nil then
        NetworkManager.mainClient = {}
    end

    if NetworkManager.mainClient.callback == nil then
        NetworkManager.mainClient.callback = {}
    end
    NetworkManager.mainClient.callback[id] = {data = requestData, callback = callback}
end

---@public
function NetworkManager:Connect()
    local netcore = CS.NetworkManager.Instance
    local sdk = CS.DH.Usdk

    local loginCenterConfig = sdk.LoginCenterConfig
    loginCenterConfig = Json.decode(loginCenterConfig)
    local addr = SplitNetAddress(loginCenterConfig.ugate_addr)
    if #addr ~= 2 then
        error(string.format("[NetworkManager] Connect failed,Not a valid ugate address: %s",loginCenterConfig))
        return
    end
    netcore:Connect(ConnectId, addr[1], tonumber(addr[2]))
    return ConnectId
end

---@private
function NetworkManager:Close()
    local netcore = CS.NetworkManager.Instance
    --- NetworkManager对象可能已经被释放
    if netcore == nil then
        return
    end
    netcore:Close(ConnectId)
    self.isConnect = false
end

---@public
function NetworkManager:Reconnect()
    local netcore = CS.NetworkManager.Instance
    netcore:Reconnect(ConnectId)
end

---@public
function NetworkManager:Release()
    local netcore = CS.NetworkManager.Instance
    if netcore == nil then
        return
    end

    self:Close()
    netcore:UnregisterHandleReceiveMessageEvent(ConnectId, HandleReceiveMessage)
    
    self:UnregisterConnectEvent(self.HandleConnectEvent)
    self:UnregisterDisconnectEvent(self.HandleDisConnectEvent)
    self:UnregisterReconnectEvent(self.HandleReconnectingEvent)
    self:UnregisterErrorMessageEvent(ProcessError)
end

---@public
--- 是否已连接
function NetworkManager:IsConnect()
    return self.isConnect
end

---@public
---@param eventCallBack function
function NetworkManager:RegisterConnectEvent(eventCallBack)
    local manager = CS.NetworkManager.Instance
    if manager == nil then
        return
    end
    manager:RegisterConnectEvent(ConnectId,eventCallBack)
end

---@public
---@param eventCallBack function
function NetworkManager:UnregisterConnectEvent(eventCallBack)
    local manager = CS.NetworkManager.Instance
    if manager == nil then
        return
    end
    manager:UnregisterConnectEvent(ConnectId,eventCallBack)
end

---@public
---@param eventCallBack function
function NetworkManager:RegisterDisconnectEvent(eventCallBack)
    local manager = CS.NetworkManager.Instance
    if manager == nil then
        return
    end
    manager:RegisterDisconnectEvent(ConnectId,eventCallBack)
end

---@public
---@param eventCallBack function
function NetworkManager:UnregisterDisconnectEvent(eventCallBack)
    local manager = CS.NetworkManager.Instance
    if manager == nil then
        return
    end
    manager:UnregisterDisconnectEvent(ConnectId,eventCallBack)
end

---@public
---@param eventCallBack function
function NetworkManager:RegisterReconnectEvent(eventCallBack)
    local manager = CS.NetworkManager.Instance
    if manager == nil then
        return
    end
    manager:RegisterReconnectEvent(ConnectId,eventCallBack)
end

---@public
---@param eventCallBack function
function NetworkManager:UnregisterReconnectEvent(eventCallBack)
    local manager = CS.NetworkManager.Instance
    if manager == nil then
        return
    end
    manager:UnregisterReconnectEvent(ConnectId,eventCallBack)
end

---@public
---@param eventCallBack function(number)
function NetworkManager:RegisterErrorMessageEvent(eventCallBack)
    local manager = CS.NetworkManager.Instance
    if manager == nil then
        return
    end
    manager:RegisterHandleReceiveErrorMessageEvent(ConnectId,eventCallBack)
end

---@public
---@param eventCallBack function(number)
function NetworkManager:UnregisterErrorMessageEvent(eventCallBack)
    local manager = CS.NetworkManager.Instance
    if manager == nil then
        return
    end
    manager:UnregisterHandleReceiveErrorMessageEvent(ConnectId,eventCallBack)
end

function NetworkManager:ProcessErrorStatus( data )
    if data and data.status and data.status < 0 then
        local ignore = ignoreErrorCodeConfig[data.status]
        if not ignore then
            local specErrorKey = specErrorCodeConfig[data.status]
            local text
            if specErrorKey then
                text = LTH:getGlobal(specErrorKey)
            else
                text = "ErrorCode = " .. data.status
            end
            TM:Show(text)
        end
    end
end
return NetworkManager