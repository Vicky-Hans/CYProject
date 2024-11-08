---
--- 文件名称:  EventHandler
--- 创建者:    yuancan.
--- 创建时间:  2021/7/19 1:33 PM
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")
local List = require("Common/List")

--模块
---@class EventHandler
local EventHandler = bpcClass("EventHandler")

--[[--
构造函数
@param #table self
@param #table t 初始化参数
]]
function EventHandler:ctor(t)
    ---@type List
    self.invocationList = List:New("function")

    local original_meta = getmetatable(self)
    original_meta.__tostring = function(table)
        return string.format("[EventHandler count is %s]",table:Count())
    end
end

---@param handler function
function EventHandler:AddListener(handler)
    if type(handler) ~= "function" then
        error("[EventHandler.AddListener] Must add handler with type function")
    end
    
    self.invocationList:Add(handler)
end
---@param handler function
function EventHandler:RemoveListener(handler)
    if type(handler) ~= "function" then
        error("[EventHandler.RemoveListener] Must add handler with type function")
    end
    
    self.invocationList:Remove(handler)
end

function EventHandler:Count()
    return self.invocationList:Count()
end

function EventHandler:Invoke(...)
    local count = self.invocationList:Count()
    for i=1,count do
        self.invocationList[i](...)
    end
end


function EventHandler:Clear()
    self.invocationList:Clear()
end

return EventHandler