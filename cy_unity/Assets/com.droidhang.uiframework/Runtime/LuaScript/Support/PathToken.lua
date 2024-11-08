---
--- 文件名称:  PathToken
--- 创建者:    yuancan.
--- 创建时间:  2021/7/21 9:56 AM
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")

--模块
---@class PathToken
local PathToken = bpcClass("PathToken")

--[[--
构造函数
]]
---@param path Path
---@param pathIndex number
function PathToken:ctor(path, pathIndex)
    ---@type Path
    self.path = path
    ---@type number
    self.pathIndex = pathIndex
end

---@public
---@return IndexedNode
function PathToken:Current()
    return self.path.nodes[self.pathIndex]
end

---@public
---@return boolean
function PathToken:HasNext()
    if self.path:Count() <= 0 or self.pathIndex >= self.path:Count() then
        return false
    end
    
    return true
end

function PathToken:PropertyName()
    return self.path:PropertyName()
end

---@public
---@return PathToken
function PathToken:NextToken()
    if not self:HasNext() then
        error("[PathToken.NextToken] index out of range exception")
        return
    end

    if self.nextToken == nil then
        self.nextToken = PathToken(self.path,self.pathIndex + 1)
    end
    
    return self.nextToken
end

return PathToken