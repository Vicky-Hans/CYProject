---
--- 文件名称:  ProcedureBase
--- 创建者:    nieshihai
--- 创建时间:  2021/9/28 11:04
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")

--模块
---@class ProcedureBase
---@field deep number
---@field parent ProcedureBase
local ProcedureBase = bpcClass("ProcedureBase")

--[[--
构造函数
]]
function ProcedureBase:ctor()

end

---@public
function ProcedureBase:Enter()
    bpcPrintf(self.__classname.." EnterProcedure")
end

---@public
--- 从子流程中退出，进入了当前流程；预留功能，当前无逻辑
--- 在连续退出流程时，不会触发中间过渡流程的ReEnter；只会触发最终流程的ReEnter
function ProcedureBase:ReEnter()
    
end

---@public
function ProcedureBase:Update(elapseSeconds, realElapseSeconds)
    
end

---@public
function ProcedureBase:Exit()
    bpcPrintf(self.__classname.." LeaveProcedure")
end

return ProcedureBase