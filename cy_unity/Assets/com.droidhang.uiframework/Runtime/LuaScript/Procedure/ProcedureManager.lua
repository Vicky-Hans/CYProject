---
--- 文件名称:  ProcedureManager
--- 创建者:    nieshihai
--- 创建时间:  2021/9/28 10:06
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")
local Dictionary = require("Common/Dictionary")
local LifeCycleManager = require("Common/LifeCycleManager")

--模块
---@class ProcedureManager
local ProcedureManager = bpcClass("ProcedureManager")

---@public
---@param config table<string,ProcedureConfigItem>
function ProcedureManager:Init(config)
    ---@type ProcedureBase
    self.current = nil
    self.config = nil
    ---@type Dictionary<string,ProcedureBase>
    self.procedureDic = Dictionary:New("string", bpcClass)

    for k, procedure in pairs(config) do
        if self.procedureDic:ContainsKey(k) then
            error("ProcedureConfig 里定义了重复的Procedure : "..k)
        end
        procedure.name = k
        self.procedureDic:Add(k, require(procedure.fileName)(self)) -- 相当于获得对应procedure的单例
        self.procedureDic[k].deep = procedure.deep
    end

    LifeCycleManager:AddToUpdateList(self)
end

---@public
function ProcedureManager:Change(procedureName)
    --- 兼容直接传入配置属性的情况
    local name = procedureName
    if type(procedureName) == "table" and procedureName.name then
        name = procedureName.name
    end
    
    if not self.procedureDic:ContainsKey(name) then
        error("没有找到名为"..name.."的Procedure，请检查ProcedureConfig")
    end
    
    print("Change procedure to "..name)

    local next = self.procedureDic[name]
    --- 检查该流程是否处于父级流程中
    if self.current == next then
        --- 已经在该流程中
        return
    end

    if self.current and self.current.deep > next.deep then
        --- 逻辑上处理出现错误
        if not self:CheckInCurrent(next) then
            error("[ProcedureManager]Logic error")
        end
        --- 需要切换的流程在父流程中，表示当前需要退出到该流程
        self:ExitToParent(next)
        return
    end
    
    --- 进入子级流程
    --- 直接更新当前流程，并触发流程进入行为；同时将当前流程的父流程更新为上一级流程
    if self.current and  self.current.deep + 1 == next.deep then
        next.parent = self.current
        self.current = next
        self.current:Enter()
    --- 同级别的procedure切换:退出当前流程，并进入新的流程
    else
        if self.current then
            self.current:Exit()
            next.parent = self.current.parent
            self.current.parent = nil
        end
   
        self.current = next
        self.current:Enter()
    end
end

function ProcedureManager:ExitCurrent()
    if self.current == nil then
        error("[ProcedureManager] ")
        return
    end
    
    self.current:Exit()
    self.current = self.current.parent
end

--- 检查该流程是否处于父级流程中
---@private
---@param procedure ProcedureBase
function ProcedureManager:CheckInCurrent(procedure)
    local current = self.current
    while(current ~= nil)
    do
        local parent = current.parent
        if parent ~= nil and procedure == parent then
            return true
        end
        current = parent
    end
    
    return false
end

--- 退出流程直到退出到指定的procedure
---@private
---@param procedure ProcedureBase
function ProcedureManager:ExitToParent(procedure)
    local current = self.current
    while(current ~= nil)
    do
        local parent = current.parent
        --- 退出当前流程
        current:Exit()
        --- 重置父节点数据
        current.parent = nil
        if parent ~= nil and procedure == parent then
            --- 相当于返回到父流程中，不再触发进入的行为
            --- 若需要触发进入的事件，需要单独处理
            self.current = procedure
            self.current:ReEnter()
            return
        end
        current = parent
    end
end

function ProcedureManager:GetProcedure(procedureName)
    if not self.procedureDic:ContainsKey(procedureName) then
        error("没有找到名为"..procedureName.."的Procedure，请检查ProcedureConfig")
    end
    
    return self.procedureDic[procedureName]
end

---@public
function ProcedureManager:Update(elapseSeconds, realElapseSeconds) 
    --- 更新当前所有流程
    local current = self.current
    while(current ~= nil)
    do
        local parent = current.parent
        current:Update(elapseSeconds, realElapseSeconds)
        current = parent
    end
end

---@public
function ProcedureManager:Shutdown()
    LifeCycleManager:RemoveFromUpdateList(self)

    --- 退出所有流程
    local current = self.current
    while(current ~= nil)
    do
        local parent = current.parent
        current:Exit()
        current.parent = nil
        current = parent
    end
    self.current = nil
end

return ProcedureManager