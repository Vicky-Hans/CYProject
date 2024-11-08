---
--- 文件名称:  LoginData
--- 创建者:    nieshihai
--- 创建时间:  2022/5/7 09:42
-------------------------------------------------------------------
--- 功能描述：
---
---

---@class LoginBindingData:ObservableObject
---@field loginAcc boolean @是否登录上帐号并且获取服务器列表成功
---@field loginGameSuccess boolean @是否登录游戏成功
---@field serverId boolean @当前选择的服务器Id

---@class ServerItemData
---@field serverData table @服务器数据
---@field roleData table @角色数据

local ProtoDataModel = require("Data/ProtoDataModel")
local ObservableList = require("Binding/ObservableList")
local ObservableObject = require("Binding/ObservableObject")
local LocalSaveData = require("Data/LocalSaveData")

--模块
---@class LoginData
local LoginData = bpcClass("LoginData")

function LoginData:Init()
    ---@type LoginBindingData
    self.data = ObservableObject()
    self.data.loginAcc = false
    self.data.loginGameSuccess = false
    self.data.serverId = -1
    
    ---@type ServerItemData[]|ObservableList
    self.servers = ObservableList()
end

function LoginData:Release()
    self.data.loginAcc = false
    self.data.loginGameSuccess = false
    self.data.serverId = -1

    self.servers:Clear()
end

function LoginData:SetLoginGameSuccess(value)
    self.data.loginGameSuccess = value
end

function LoginData:SetSvrId(sid)
    self.data.serverId = sid
end

function LoginData:HasSvrRole()
    local svr = self:GetSvrInfoBySid(self.data.serverId)
    
    return svr.roleData ~= nil;
end

function LoginData:ParseServersAndRolesInfo(roles,servers)
    self.servers:Clear()
    --- 记录推荐服
    self.recommend = -1
    local recommendLevel = 0
    local ctime = 0

    --- 设置为推荐服
    local SetCommendServer = function(server)
        self.recommend = server.sid
        recommendLevel = server.recommend
        ctime = server.ctime
    end
    
    -- 解析服务器信息
    for _, serverData in pairs(servers) do
        --- 更新推荐服
        if serverData.recommend > recommendLevel then
            SetCommendServer(serverData)
        elseif serverData.recommend == recommendLevel and serverData.ctime > ctime then
            SetCommendServer(serverData)
        end
        
        local server = ProtoDataModel(serverData)

        local roleInServer = nil
        if #roles > 0 then
            for _,role in pairs(roles) do
                if role.sid == server.sid then
                    roleInServer = role
                end
            end
        end
        
        local serverItem = {
            serverData = server,
            roleData = roleInServer    -- roleData可能为空，该服务区无角色
        }

        self.servers:Add(serverItem)
    end
    
    self.data.loginAcc = true
    self:GetDefaultServerId()
end

function LoginData:GetDefaultServerId()
    local function genNewSvrId()
        if self.recommend > 0 then
            return self.recommend
        end
        
        return -1
    end

    local serverId = LocalSaveData:GetRecordServerId()

    if serverId < 0 or self:GetSvrInfoBySid(serverId) == nil then
        self.data.serverId = genNewSvrId()

        if self.data.serverId >= 0 then
            LocalSaveData:SaveRecordServerId(self.data.serverId)
        end
    else
        self.data.serverId = serverId
    end
end

---@return ServerItemData
function LoginData:GetSvrInfoBySid(sid)
    local svr = self.servers:Find(function(server)
        return sid == server.serverData.sid
    end)
    
    return svr
end

return LoginData