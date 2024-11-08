---
--- 文件名称:  ChainedSource
--- 创建者:    yuancan.
--- 创建时间:  2021/7/21 2:27 PM
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")
local EventHandler = require("Support/EventHandler")
local ChainedSourceEntry = require("Binding/ChainedSourceEntry")
--模块
---@class ChainedSource
local ChainedSource = bpcClass("ChainedSource")

--[[--
构造函数
]]
---@param source any
---@param token PathToken
function ChainedSource:ctor(source, token)
    self.source = source
    ---@type PathToken
    self.token = token
    ---@type EventHandler
    self.valueChanged = EventHandler()
    ---@type table<ChainedSourceEntry>
    self.entries = {}
    ---@type number
    self.chainCount = token.path:Count()
    self:Bind(source, token)

    ---override setter and getter
    ---when target try to get the source property value
    ---we redirect it to GetValue or SetValue(value) member functions
    local __index = function(t, k)
        if t.propertyName == k then
            return t:GetValue()
        end

        local value = nil
        if string.find(k, "^_") == 1 then
            value = getmetatable(t)[k]
        else
            local v = rawget(t, "_attr_" .. k)
            if v ~= nil then
                value = v
            else
                value = getmetatable(t)[k]
            end
        end
        return value
    end
    local __newindex = function(t, k, v)
        if t.propertyName == k then
            t:SetValue(v)
            return
        end

        if type(v) == "function" or string.find(k, "^_") == 1 then
            rawset(t, k, v)
        else
            rawset(t, "_attr_" .. k, v)
        end
    end
    local original_meta = getmetatable(self)
    local meta = setmetatable({ __index = __index, __newindex = __newindex }, original_meta)
    setmetatable(self, meta)
end

---@param value any
function ChainedSource:SetValue(value)
    ---@type ChainedSourceEntry
    local lastEntry = self.entries[self.chainCount]
    if lastEntry == nil then
        return
    end
    return lastEntry:SetValue(value)
end

---@return any
function ChainedSource:GetValue()
    ---@type ChainedSourceEntry
    local lastEntry = self.entries[self.chainCount]
    if lastEntry == nil then
        return nil
    end
    return lastEntry:GetValue()
end

---@return any
function ChainedSource:GetSource()
    ---@type ChainedSourceEntry
    local lastEntry = self.entries[self.chainCount]
    if lastEntry == nil then
        return nil
    end
    return lastEntry:GetSource()
end

---@param source any
---@param token PathToken
function ChainedSource:Bind(source, token)
    local index = token.pathIndex
    ---@type ChainedSourceEntry
    local entry = ChainedSourceEntry(source, token)
    ---@type IndexedNode
    local node = token:Current()
    self.propertyName = node.value

    ---release old
    self.entries[index] = nil
    self.entries[index] = entry

    if token:HasNext() then
        entry:SetHandler(function(entrySource, args)
            if entry == nil or entrySource ~= entry.source then
                return
            end

            self:Rebind(index)
        end)

        local child = source[node.value]
        if child ~= nil then
            self:Bind(child, token:NextToken())
        else
            self:RaiseValueChanged()
        end
    else
        entry:SetHandler(function(sender, args)
            self:RaiseValueChanged()
        end)
        self:RaiseValueChanged()
    end
end

---@private
---@param index number
function ChainedSource:Rebind(index)
    for i = index + 1, self.chainCount do
        ---@type ChainedSourceEntry
        local entryItem = self.entries[i]
        if entryItem ~= nil then
            entryItem:SetSource(nil)
        end
    end

    ---@type ChainedSourceEntry
    local entry = self.entries[index]
    local source = entry.source[entry.propertyName]
    if source == nil then
        self:RaiseValueChanged()
        return
    end

    self:Bind(source, entry.token:NextToken())
end

function ChainedSource:Unbind()
    for i = 1, self.chainCount do
        ---@type ChainedSourceEntry
        local entry = self.entries[i]
        if entry ~= nil then
            entry:Dispose()
        end
        self.entries[i] = nil
    end
end

---@private
function ChainedSource:RaiseValueChanged()
    self.valueChanged:Invoke(self, self.propertyName)
end

---@param propertyName string
---@param action function
function ChainedSource:subscribe(propertyName, action)
    self.valueChanged:AddListener(action)
end

---@param propertyName string
---@param action function
function ChainedSource:unsubscribe(propertyName, action)
    self.valueChanged:RemoveListener(action)
end

return ChainedSource