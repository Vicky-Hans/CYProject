---
--- 文件名称:  DataContext
--- 创建者:    yuancan.
--- 创建时间:  2021/7/15 9:49 AM
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")
local List = require("Common/List")

--模块
---@class DataContext
local DataContext = bpcClass("DataContext")

--[[--
构造函数
@param #table self
@param #table t 初始化参数
]]
function DataContext:ctor(t)
    self.source = nil
    self.bindingList = List:New()
end

function DataContext:SetSource(source)
    if self == nil then
        return
    end
    
    if source == nil then
        self:Unbind()
    else
        self:BindSource(source)
    end
end

function DataContext:IsBound()
    return self.source ~= nil
end

function DataContext:AddBinding(binding)
    if binding == nil then
        return
    end

    if self.bindingList:Contains(binding) then
        return
    end
    
    self.bindingList:Add(binding)
end

function DataContext:Build()
    if self:IsBound() then
        self:BindSource(self.source)
    end
end

function DataContext:RemoveBinding(binding)
    if binding == nil then
        return
    end

    if not self.bindingList:Contains(binding) then
        return
    end

    self.bindingList:Remove(binding)

    if self:IsBound() then
        self:Unbind(self.source)
    end
end

function DataContext:BindSource(source)
    if self:IsBound() then
        self:Unbind()
    end
    
    self.source = source;
    
    self.bindingList:ForEach(function(item)
        item:Bind(source)
    end)
    
    local manager = require("Binding/BindingManager")
    manager.dataContext[self] = self
end

function DataContext:Unbind()
    if not self:IsBound() then
        return
    end
    
    self.source = nil
    self.bindingList:ForEach(function(item)
        if item == nil then
            return
        end
        item:Unbind()
    end)

    local manager = require("Binding/BindingManager")
    manager.dataContext[self] = nil
end

function DataContext:RefreshBindings()
    self.bindingList:ForEach(function(item)
        if item == nil then
            return
        end
        item:UpdateTarget()
    end)
end

return DataContext