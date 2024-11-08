---
--- 文件名称:  TabViewBinding
--- 创建者:    nieshihai
--- 创建时间:  2021/10/20 14:02
-------------------------------------------------------------------
--- 功能描述：
--- 是TwoWay的方式，C#层选择一个Toggle时需要更新SourceModel的CurrentTabOption
--- SourceModel的CurrentTabOption手动赋值时需要更新C#层Toggle的状态

local TabOptionalItem = require("Binding/TabView/TabOptionalItem")
local List = require("Common/List")
local BindingManager = require("Binding/BindingManager")

--模块
---@class TabViewBinding
local TabViewBinding = bpcClass("TabViewBinding")

--[[--
构造函数
]]
function TabViewBinding:ctor()
    self.currentIndex = 0
    
    ---@type List<TabOptionalItem>
    self.tabOptionItems = List:New(TabOptionalItem)
    
    self.propertyChanged = function(sender,propertyName) self:OnPropertyChanged(sender,propertyName) end
    self.onSelected = function(index) self:Select(index, true) end
end

---@public
---@return TabViewBinding
function TabViewBinding:Target(toggleGroup, contentParent)
    assert(toggleGroup ~= nil,"[TabViewBinding]Target toggleGroup is nil")
    self.toggleGroup = toggleGroup
    self.contentParent = contentParent
    return self
end

---@public
---@return TabViewBinding
function TabViewBinding:To(sourcePath)
    if sourcePath == nil then
        error("[TabViewBinding.To]source path is nil")
        return
    end

    self.sourcePath = sourcePath
    self.sourcePropertyName = BindingManager:GetPropertyPath(sourcePath)

    return self
end

---@public
---@param dataContext DataContext
---@return TabViewBinding
function TabViewBinding:Build(dataContext)
    assert(dataContext ~= nil,"[DropdownBinding]Build dataContenxt is nil")
    dataContext:AddBinding(self)
    return self
end

---@public
---@return boolean
function TabViewBinding:IsBound()
    return self.source ~= nil
end

---@public
---@param source any
function TabViewBinding:Bind(source)
    if source == nil then
        error("[TabViewBinding.Bind]source is nil")
        return
    end

    if self:IsBound() then
        self:Unbind()
    end

    local bindingSource = source
    if bindingSource == nil then
        error("binding source is null")
        return
    end

    self.source = bindingSource
    self.source:subscribe(self.sourcePropertyName, self.propertyChanged)

    self:InitValue()
end

---@public
function TabViewBinding:Unbind()
    if not self:IsBound() then
        return
    end

    self.source:unsubscribe(self.sourcePropertyName,self.propertyChanged)
    self.source = nil
    self.currentIndex = 0
    
    self.tabOptionItems:ForEach(function(item) item:UnBind() end)
end

---@public 加入属于这个ToggleGroup下的Toggle
---@param togglePath string
---@param template GameObject
---@param vmPath string
function TabViewBinding:AddOptionSettings(togglePath, template,vmPath)
    local toggle = self.toggleGroup.transform:GetAutoInjectNode(togglePath, togglePath, "UnityEngine.UI.Toggle")
    if toggle == nil then
        error("在"..self.toggleGroup.name.."没有找到"..togglePath.."的Toggle")
    end

    local index = self.tabOptionItems:Count() + 1
    self.tabOptionItems:Add(TabOptionalItem(index, toggle, self.contentParent, template, self.onSelected,vmPath))
end

function TabViewBinding:InitValue()
    if not self:IsBound() then
        return
    end

    local index = self.source[self.sourcePropertyName]
    self:Select(index, false)
    
    self.tabOptionItems:ForEach(function(item)
        item:Bind(self.source)
    end)
end

---@public
---@param sender any
---@param propertyName string
function TabViewBinding:OnPropertyChanged(sender, propertyName)
    assert(self:IsBound(),"[TabViewBinding]Try to notify on property changed on a unbind binding")
    if sender ~= self.source then
        return
    end

    if propertyName ~= nil and self.sourcePropertyName ~= propertyName then
        -- ignore invalid propertyName
        return
    end

    local index = self.source[self.sourcePropertyName]
    self:Select(index, false)
end

---@public 选择某个Tab
---@param bNeedUpdateSource boolean @如果为true说明是界面点击toggle的，所以更新VM里绑定的值。为false是VM改变，我们需要刷新UI上的Toggle的状态
function TabViewBinding:Select(index, bNeedUpdateSource)
    if index <= 0 or index > self.tabOptionItems:Count() then
        error("the index "..index.." is out of range!")
    end

    if self.currentIndex == index then
        return
    end
    
    self:OnUnSelect(bNeedUpdateSource)
    self:OnSelect(index, bNeedUpdateSource)
end

---@private 选择某个Tab
function TabViewBinding:OnSelect(index, bNeedUpdateSource)
    self.currentIndex = index
    self.tabOptionItems[index]:Select(not bNeedUpdateSource)

    if bNeedUpdateSource then
        -- 通知Source的值
        if not self:IsBound() then
            return
        end

        self.source[self.sourcePropertyName] = index
    end
end

---@private 选择某个Tab
function TabViewBinding:OnUnSelect(bNeedUpdateSource)
    if self.currentIndex <= 0 or self.currentIndex > self.tabOptionItems:Count() then
        return
    end
    
    self.tabOptionItems[self.currentIndex]:UnSelect(not bNeedUpdateSource)
end

return TabViewBinding