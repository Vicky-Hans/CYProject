---
--- 文件名称:  DropdownBinding
--- 创建者:    yuancan.
--- 创建时间:  2021/9/16 2:57 PM
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")
local OptionData = CS.UnityEngine.UI.Dropdown.OptionData
local enableDebug = DataBindingDebug
local BindingManager = require("Binding/BindingManager")

--模块
---@class DropdownBinding
local DropdownBinding = bpcClass("DropdownBinding")

local CollectionChanged = "CollectionChanged"

function DropdownBinding:ctor(t)
    self.source = nil
    self.sourcePath = nil
    self.sourcePropertyName = nil
    ---@type ObservableList
    self.itemsSource = nil
    ---@type function
    self.collectionChanged = function(sender,args) self:OnCollectionChanged(sender,args)  end
    self.propertyChanged = function(sender,propertyName) self:OnPropertyChanged(sender,propertyName) end
end

---@public
---@return boolean
function DropdownBinding:IsBound()
    return self.itemsSource ~= nil
end

---@public
---@return DropdownBinding
function DropdownBinding:To(sourcePath)
    if sourcePath == nil then
        error("[CollectionBinding.To]source path is nil")
        return
    end

    self.sourcePath = sourcePath
    self.sourcePropertyName = BindingManager:GetPropertyPath(sourcePath)

    return self
end

---@public
---@return DropdownBinding
function DropdownBinding:Target(dropdown)
    assert(dropdown ~= nil,"[DropdownBinding]Target dropdown is nil")
    self.dropdown = dropdown
    return self
end

---@public
---@param dataContext DataContext
---@return DropdownBinding
function DropdownBinding:Build(dataContext)
    assert(dataContext ~= nil,"[DropdownBinding]Build dataContenxt is nil")
    dataContext:AddBinding(self)
    return self
end

---@public
---@param source any
function DropdownBinding:Bind(source)
    if source == nil then
        error("[DropdownBinding.Bind]source is nil")
        return
    end

    if self:IsBound() then
        self:Unbind()
    end

    -- handle nested property
    local bindSource = source
    if bindSource == nil then
        error("[DropdownBinding.Bind]bind source is nil")
        return
    end

    ---@type ObservableList
    local collectionObject = bindSource[self.sourcePropertyName]
    if collectionObject == nil or collectionObject.__classname ~= "ObservableList" then
        error("[DropdownBinding.Bind]Binding failed,collection object is nil or not a ObservableList type object "..bindSource.__classname)
        return
    end

    self.source = bindSource;
    self.itemsSource = collectionObject;

    collectionObject:subscribe(CollectionChanged,self.collectionChanged)
    bindSource:subscribe(self.sourcePropertyName,self.propertyChanged)
    self:RefreshView()
end

---@public
function DropdownBinding:Unbind()
    if not self:IsBound() then
        return
    end

    self.itemsSource:unsubscribe(CollectionChanged,self.collectionChanged)
    self.source:unsubscribe(self.sourcePropertyName,self.propertyChanged)
    
    self:RefreshView()

    self.source = nil
    self.itemsSource = nil
end

---@public
---@param sender any
---@param propertyName string
function DropdownBinding:OnPropertyChanged(sender, propertyName)
    assert(self:IsBound(),"[CollectionBinding]Try to notify on property changed on a unbind binding")
    if sender ~= self.source then
        return
    end

    if propertyName ~= nil and self.sourcePropertyName ~= propertyName then
        -- ignore invalid propertyName
        return
    end

    local collectionObject = self.source[self.sourcePropertyName]
    if collectionObject == nil or collectionObject.__classname ~= "ObservableList" then
        error("[CollectionBinding.Bind]Binding failed,collection object is nil or not a ObservableList type object")
        return
    end

    if collectionObject == self.itemsSource then
        -- same collectio no need to update
        return
    end
    
    self:RefreshView()
end

---@public
---@param newSource any
function DropdownBinding:ChangeItemsSource(newSource)
    if self.itemsSource ~= nil then
        self.itemsSource:unsubscribe(self.sourcePropertyName,self.collectionChanged)
    end

    -- update new items
    self.itemsSource = newSource
    self.itemsSource:subscribe(self.sourcePropertyName,self.collectionChanged)
    self:RefreshView()
end

---@public
---@param sender any
---@param args {action,newItems,oldItems}
function DropdownBinding:OnCollectionChanged(sender, args)
    assert(args ~= nil,"[CollectionBinding.OnCollectionBinding] args is nil")
    if enableDebug then
        bpcPrintf("[CollectionBinding.OnCollectionChanged] action is %s",args.action)
    end
    self:RefreshView()
end

---@private
function DropdownBinding:RefreshView()
    if self.dropdown == nil then
        return
    end

    if self.itemsSource == nil then
        self.dropdown.options:Clear()
        return
    end
    
    self.dropdown.options:Clear()
    self.itemsSource:ForEach(function(item)
        local option = OptionData()
        local type = type(item)
        if type == 'string' then
            option.text = item
        else
            option.text = item.title
        end

        self.dropdown.options:Add(option)
    end)
end

return DropdownBinding