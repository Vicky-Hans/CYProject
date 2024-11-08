---
--- 文件名称:  LifeCycleManager
--- 创建者:    nieshihai
--- 创建时间:  2021/10/8 16:39
-------------------------------------------------------------------
--- 功能描述：
--- 管理lua view的update和lateUpdate
---

require("Common/System")

--模块
---@class LifeCycleManager
local LifeCycleManager = bpcClass("LifeCycleManager")
local List = require("Common/List")
local GameFramework = CS.DHFramework.GameFramework
local get_module_generic = xlua.get_generic_method(GameFramework, 'GetModule')
local GetModule = get_module_generic(CS.DH.UIFramework.LuaFrameworkModule)

--[[--
构造函数
]]
function LifeCycleManager:Init()
    if self.init then
        return
    end
    
    self.init = true
    ---@type List
    self.updateList = List:New("class")
    ---@type List
    self.lateUpdateList = List:New("class")
    
    local module = GetModule(GameFramework.Instance)
    module:AddModule(self)
end

function LifeCycleManager:Update(elapseSeconds, realElapseSeconds)
    self.updateList:ForEach(function(view) view:Update(elapseSeconds, realElapseSeconds) end)
end

function LifeCycleManager:LateUpdate()
    self.lateUpdateList:ForEach(function(view) view:LateUpdate() end)
end

function LifeCycleManager:AddToUpdateList(view)
    if not self.updateList:Contains(view) then
        self.updateList:Add(view)
    end
end

function LifeCycleManager:RemoveFromUpdateList(view)
    if self.updateList:Contains(view) then
        self.updateList:Remove(view)
    end
end

function LifeCycleManager:AddToLateUpdateList(view)
    if not self.lateUpdateList:Contains(view) then
        self.lateUpdateList:Add(view)
    end
end

function LifeCycleManager:RemoveFromLateUpdateList(view)
    if self.lateUpdateList:Contains(view) then
        self.lateUpdateList:Remove(view)
    end
end

function LifeCycleManager:Release()
    local module = GetModule(GameFramework.Instance)
    module:RemoveModule(self)
    self.init = false
end

return LifeCycleManager