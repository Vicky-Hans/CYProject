---
--- 文件名称:  Path
--- 创建者:    yuancan.
--- 创建时间:  2021/7/21 9:39 AM
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")
local PathToken = require("Support/PathToken")
local List = require("Common/List")
local IndexedNode = require("Support/IndexedNode")
local StringIndexedNode = require("Support/StringIndexedNode")
local MemberNode = require("Support/MemberNode")

--模块
---@class Path
local Path = bpcClass("Path")

--[[--
构造函数
@param #table self
@param #table t 初始化参数
]]
function Path:ctor()
    ---@type List<IndexedNode| MemberNode>
    self.nodes = List:New("PathNode")
    ---@type PathToken
    self.token = nil

    local orginal_meta = getmetatable(self)
    orginal_meta.__tostring = function(t)
        local str = '[Path]'..tostring(t.nodes)
        return str
    end
end

---@public
---@return boolean
function Path:Count()
    return self.nodes:Count()
end

---@public
---@param pathNode IndexedNode
function Path:Append(pathNode)
    self.nodes:Add(pathNode)
end

---@public
---@param pathNode IndexedNode
function Path:Prepend(pathNode)
    self.nodes:Insert(1,pathNode)
end

---@public
---@param indexValue any
function Path:PrependIndexed(indexValue)
    local t = type(indexValue)
    if t == "string" then
        self:Prepend(StringIndexedNode(indexValue))
    elseif t == "number" then
        self:Prepend(IndexedNode(indexValue))
    else
        error("[Path.PrependIndexed] not support type "..t)
    end
end

---@public
---@param indexValue any
function Path:AppendIndexed(indexValue)
    local t = type(indexValue)
    if t == "string" then
        self:Append(StringIndexedNode(indexValue))
    elseif t == "number" then
        self:Append(IndexedNode(indexValue))
    else
        error("[Path.PrependIndexed] not support type "..t)
    end
end

---@public
---@return PathToken
function Path:AsPathToken()
    if self.token ~= nil then
        return self.token
    end

    if self.nodes:Count() <= 0 then
        error("[Path.AsPathToken] The path node is empty "..t)
    end
    
    self.token = PathToken(self,1)
    return self.token
end

---@return string
function Path:PropertyName()
    return self.nodes[self.nodes:Count()].value
end

return Path