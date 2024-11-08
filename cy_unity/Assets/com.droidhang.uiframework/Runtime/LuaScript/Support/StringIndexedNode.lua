---
--- 文件名称:  StringIndexedNode
--- 创建者:    yuancan
--- 创建时间:  2022/1/12 3:16 PM
-------------------------------------------------------------------
--- 功能描述：
---
---

--模块
---@class StringIndexedNode
---@field value
local StringIndexedNode = bpcClass("StringIndexedNode")

--[[--
构造函数
]]
function StringIndexedNode:ctor(name)
    rawset(self,"value",name)
    getmetatable(self).__newindex = function(t,k,v)
        error("[StringIndexedNode] is ready only class")
    end

    getmetatable(self).__tostring = function(t)
        return tostring(t.value)
    end
end

return StringIndexedNode