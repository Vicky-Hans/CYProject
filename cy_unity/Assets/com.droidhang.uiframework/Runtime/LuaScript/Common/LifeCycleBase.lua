---
--- 文件名称:  LifeCycleBase
--- 创建者:    nieshihai
--- 创建时间:  2021/10/8 17:42
-------------------------------------------------------------------
--- 功能描述：
--- 需要update和lateUpdate的对象统一注册到一个脚本里管理
---
local LifeCycleManager = require("Common/LifeCycleManager")


--模块
---@class LifeCycleBase
local LifeCycleBase = bpcClass("LifeCycleBase")

function LifeCycleBase:RegisterLifeCycle()
    if self.onUpdate ~= nil then
        LifeCycleManager:AddToUpdateList(self)
    end

    if self.onLateUpdate ~= nil then
        LifeCycleManager:AddToLateUpdateList(self)
    end
end

function LifeCycleBase:UnRegisterLifeCycle(...)
    if self.onUpdate ~= nil then
        LifeCycleManager:RemoveFromUpdateList(self)
    end

    if self.onLateUpdate ~= nil then
        LifeCycleManager:RemoveFromLateUpdateList(self)
    end
end

function LifeCycleBase:Update(elapseSeconds, realElapseSeconds)
    if self.onUpdate ~= nil then
        self:onUpdate(elapseSeconds, realElapseSeconds)
    end
end

function LifeCycleBase:LateUpdate()
    if self.onLateUpdate ~= nil then
        self:onLateUpdate()
    end
end

return LifeCycleBase