---
--- 文件名称:  IndexedNode
--- 创建者:    yuancan.
--- 创建时间:  2021/7/21 9:47 AM
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")

--模块
---@class IndexedNode
---@field value any
local IndexedNode = bpcClass("IndexedNode")

---@param value any
function IndexedNode:ctor(value)
    rawset(self,"value",value)
    getmetatable(self).__newindex = function(t,k,v)
        error("[MemberNode] is ready only class")
    end

    getmetatable(self).__tostring = function(t)
        return tostring(t.value)
    end
end

return IndexedNode