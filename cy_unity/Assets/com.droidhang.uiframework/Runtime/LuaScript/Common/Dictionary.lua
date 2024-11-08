---
--- 文件名称:  Dictionary
--- 创建者:    yuancan.
--- 创建时间:  2021/7/15 9:37 AM
-------------------------------------------------------------------
--- 功能描述：

local List = require("Common/List")

---@field keys List | any[]
---@class Dictionary
local Dictionary = bpcClass("Dictionary")
local __newindex = function(t,k,v)
    error("[Dictionary]Not support modify directly")
end

local __index = function(t,k)
    local value = Dictionary[k]
    if value ~= nil then
        return value
    end
    local container = rawget(t,"container")
    local result =  rawget(container,k)
    if result == nil then
        return rawget(t,k)
    else
        return result
    end
end

function Dictionary:New(tk, tv)
    local o = {keyType = tk, valueType = tv,container = {},keys = List:New()}
    o.itemCount = 0
    self.__tostring = function(table)
        local str = "[Dictionary] Count %s Value -> {%s}"
        local dataStr = ""
        table:ForEach(function(key,item)
            dataStr = dataStr..string.format("[key:%s value:%s]",key,item)
        end)

        return string.format(str,table:Count(),dataStr)
    end
    self.__newindex = __newindex
    self.__index = __index
    setmetatable(o, self)
    return o
end

function Dictionary:Add(key, value)
    if value == nil then
        error(key..tostring(value))
    end
    
    if self.container[key] == nil then
        self.container[key] = value
        self.itemCount = self.itemCount + 1
        self.keys:Add(key)
    else
        self.container[key] = value
    end
end

---@param collection Dictionary | table<any, any>
function Dictionary:AddRange(collection)
    if not ((collection.__classname == nil and type(collection) == "table") or collection.__classname == "Dictionary") then
        error("collection is invalid!")
    end
    
    local addCount = 0
    --- 纯的table
    if collection.__classname == nil and type(collection) == "table" then
        for k, v in pairs(collection) do
            self:Add(k, v)
            addCount = addCount + 1
        end
    else
        collection:ForEach(function(key,value)
            self:Add(key, value)
            addCount = addCount + 1
        end)
    end
    
    return addCount
end

function Dictionary:Clear()
    for k, _ in pairs(self.container) do
        self.container[k] = nil
    end
    self.itemCount = 0
    self.keys:Clear()
end

function Dictionary:ContainsKey(key)
    if key == nil then
        error("Key can not be nil")
        return
    end
    
    return self.container[key] ~= nil
end

function Dictionary:ContainsValue(value)
    for k, v in pairs(self.container) do
        if v == value then
            return true
        end
    end
    return false
end

---@return number
function Dictionary:Count()
    return self.itemCount
end

---@param keys any[]
function Dictionary:RemoveRange(keys)
    for i, v in ipairs(keys) do
        self:Remove(v)
    end
end

---@param key any
function Dictionary:Remove(key)
    if self.container[key] == nil then
        return
    end
    
    self.container[key] = nil
    self.itemCount = self.itemCount - 1
    self.keys:Remove(key)
end

---@generic T, K
---@param action fun(key:T,value:K)
function Dictionary:ForEach(action)--遍历，参数function
    if (action == nil or type(action) ~= 'function') then
        print('action is invalid!')
        return
    end

    self.keys:ForEach(function(item)
        local v = self.container[item]
        action(item,v)
    end)
end

function Dictionary:Find(predicate)
    for k, v in pairs(self.container) do
        if predicate(k,v) then
            return k,v
        end
    end
    
    return nil,nil
end

function Dictionary:Keys()
    return self.keys
end

function Dictionary:Values()
    local values = {}
    local index = 1
    for _, v in pairs(self.container) do
        values[index] = v
        index = index + 1
    end
end

function Dictionary:KeyType()
    return self.keyType
end

function Dictionary:ValueType()
    return self.valueType
end

return Dictionary