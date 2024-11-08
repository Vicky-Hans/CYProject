---
--- 文件名称:  MemberNode
--- 创建者:    yuancan.
--- 创建时间:  2021/7/21 1:51 PM
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")

--模块
---@class MemberNode
local MemberNode = bpcClass("MemberNode")

--[[--
构造函数
@param #table self
@param #table t 初始化参数
]]
---@param name string
function MemberNode:ctor(name)
    rawset(self,"value",name)
    getmetatable(self).__newindex = function(t,k,v)
        error("[MemberNode] is ready only class")
    end

    getmetatable(self).__tostring = function(t)
        return tostring(t.value)
    end
end

return MemberNode