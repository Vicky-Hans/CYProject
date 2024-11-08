---
--- 文件名称:  ChainedSourceEntry
--- 创建者:    yuancan.
--- 创建时间:  2021/7/21 2:32 PM
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")

--模块
---@class ChainedSourceEntry
local ChainedSourceEntry = bpcClass("ChainedSourceEntry")

local CollectionChanged = "CollectionChanged"

--[[--
构造函数
]]
---@param source any
---@param token PathToken
function ChainedSourceEntry:ctor(source, token)
    self.token = token
    self.handler = nil
    self.propertyName = token:Current().value
    self.onValueChanged = function()
        if self.handler == nil then
            return
        end
        self.handler(self.source,nil)
    end
    self:SetSource(source)
end

---@return any
function ChainedSourceEntry:GetValue()
    if self.source == nil then
        return nil
    end
   
    return self.source[self.propertyName]
end

---@return any
function ChainedSourceEntry:GetSource()
    return self.source
end

---@param value any
function ChainedSourceEntry:SetValue(value)
    if self.source == nil then
        return
    end

    self.source[self.propertyName] = value
end

---@param source any
function ChainedSourceEntry:SetSource(source)
    if self.source == source then
        return
    end

    if self.source ~= nil and self.source.unsubscribe ~= nil then
        local className = self.source.__classname
        if className == "ObservableList" or  className == "ObservableDictionary" then
            self.source:unsubscribe(CollectionChanged,self.onValueChanged)
        else
            self.source:unsubscribe(self.propertyName,self.onValueChanged)
        end
    end

    if source ~= nil and source.subscribe ~= nil then
        local className = source.__classname
        if source.__classname == "ObservableList" or  className == "ObservableDictionary" then
            source:subscribe(CollectionChanged,self.onValueChanged)
        else
            source:subscribe(self.propertyName,self.onValueChanged)
        end
    end
    
    self.source = source
end

---@param handler function
function ChainedSourceEntry:SetHandler(handler)
    if self.handler == handler then
        return
    end
    
    self.handler = handler
end

function ChainedSourceEntry:Dispose()
    self:SetSource(nil)
    self:SetHandler(nil)
    self.token = nil
end

return ChainedSourceEntry