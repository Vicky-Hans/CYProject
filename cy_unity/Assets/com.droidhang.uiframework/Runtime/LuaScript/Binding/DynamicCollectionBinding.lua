---
--- 文件名称:  DynamicCollectionBinding
--- 创建者:    yuancan.
--- 创建时间:  2021/7/19 9:41 AM
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")
local LuaBindingTargetWrapper =  CS.UIFramework.LuaBindingTargetWrapper
local DynamicViewItem = require("Binding/DynamicViewItem")
local Binding = require("Binding/Binding")
local DynamicCollectionView = require("CollectionView/DynamicCollectionView")
local IndexerEvent = "Item[]"
local CollectionChanged = "CollectionChanged"
local OnPropertyChanged = "OnPropertyChanged"
--模块
---@class DynamicCollectionBinding
local DynamicCollectionBinding = bpcClass("DynamicCollectionBinding")

--[[--
构造函数
@param #table self
@param #table t 初始化参数
]]
function DynamicCollectionBinding:ctor(t)
    ---@type any
    self.source = nil
    ---@type string
    self.sourcePath = nil
    ---@type ObservableList
    self.itemsSource = nil
    ---@type DefaultDynamicViewFactory
    self.factory = nil
    ---@type List
    self.pendingList = List:New()
    ---@type Dictionary<number,DynamicViewItem>
    self.bindingDictionary = Dictionary:New("int","Item")
    self.collectionChanged = function(sender,args) self:OnCollectionChanged(sender,args)  end
    self.propertyChanged = function(sender, propertyName, oldValue, newValue)
        self:OnPropertyChanged(sender, propertyName, oldValue, newValue)
    end
end

---@public
function DynamicCollectionBinding:BindToProperty()
    return self.sourcePath ~= nil
end

---@public
---@return number
function DynamicCollectionBinding:Count()
    if self.collectionView ~= nil then
        return self.collectionView.indexedList:Count()
    end
    return self.itemsSource:Count()
end

---@public
---@return boolean
function DynamicCollectionBinding:IsBound()
    return self.itemsSource ~= nil
end

---@return DynamicCollectionBinding
---@param sourcePath string
---@param factory any
---@param config {itemSize,itemCount,itemSpace}
function DynamicCollectionBinding:To(sourcePath, factory, config)
    if factory == nil then
        error("[CollectionBinding.To]factory is nil")
        return
    end

    if sourcePath == nil then
        error("[CollectionBinding.To]source path is nil")
        return
    end

    if self.content == nil then
        error("[CollectionBinding.To]Should call CollectionBinding:Target() first")
        return
    end

    self.factory = factory
    self.factory.content = self.content
    self.factory.viewTemplate = self.viewTemplate
    --- preload prefab 
    self.factory:PrepareTemplate()
    -- clear cached template and view content
    self.content = nil
    self.viewTemplate = nil
    self.sourcePath = sourcePath
    self.sourcePropertyName = UDH.BindingManager:GetPropertyPath(sourcePath)
    self.factory:Init(self,config)
    return self
end

---@return DynamicCollectionBinding
function DynamicCollectionBinding:WithHorizontal()
    self.factory:WithHorizontal()
    return self
end

---@public
---@param collectionViewPath string
---@return DynamicCollectionBinding
function DynamicCollectionBinding:WithCollectionView(collectionViewPath)
    ---@type DynamicCollectionView
    self.collectionView = DynamicCollectionView(self.itemsSource,self)
    self.collectionView.viewChange = function()
        self.factory:UpdateView()
    end
    return self
end

---@return DynamicCollectionBinding
---@param content Transform
---@param viewTemplate GameObject
function DynamicCollectionBinding:Target(content, viewTemplate)
    assert(content ~= nil,"[CollectionBinding]Target content is nil")
    assert(viewTemplate ~= nil,"[CollectionBinding]Target viewTemplate is nil")
    self.content = content
    self.viewTemplate = viewTemplate
    return self
end
---@param dataContext DataContext
---@return DynamicCollectionBinding
function DynamicCollectionBinding:Build(dataContext)
    assert(dataContext ~= nil,"[CollectionBinding]Build dataContenxt is nil")
    if self.collectionView ~= nil then
        self.collectionView:Build(dataContext)
    end
    
    dataContext:AddBinding(self)
    return self
end

---@public
---@param source ObservableObject
function DynamicCollectionBinding:Bind(source)
    if self:IsBound() then
        self:Unbind()
    end
    
    ---@type ObservableList
    local collectionObject = nil
    
    if self:BindToProperty() then
        collectionObject = source[self.sourcePath]
    else
        collectionObject = source
    end
    
    if collectionObject == nil or collectionObject.__classname ~= "ObservableList" then
        error("[CollectionBinding.Bind]Binding failed,collection object is nil or not a ObservableList type object")
        return
    end

    self.source = source;
    self.itemsSource = collectionObject;

    if self.collectionView then
        self.collectionView.bindingSource = collectionObject
    end

    collectionObject:subscribe(CollectionChanged,self.collectionChanged)
    if self:BindToProperty() then
        source:subscribe(self.sourcePropertyName,self.propertyChanged)
    end

    self:CreateCollection()
end

---@public
function DynamicCollectionBinding:Unbind()
    if not self:IsBound() then
        return
    end

    self.itemsSource:unsubscribe(CollectionChanged,self.collectionChanged)
    if self:BindToProperty() then
        self.source:unsubscribe(self.sourcePropertyName,self.propertyChanged)
    end
    
    self:ClearCollection()
    --clear source
    self.source = nil
    self.itemsSource = nil
end

---@private
function DynamicCollectionBinding:CreateCollection()
    self:RefreshView()
end

---@private
function DynamicCollectionBinding:ClearCollection()
    self:ClearItems()
    self:RefreshView()
end

---@private
---@param newSource any
function DynamicCollectionBinding:ChangeItemsSource(newSource)
    if self.itemsSource ~= nil then
        self.itemsSource:unsubscribe(CollectionChanged,self.collectionChanged)
    end
    
    if self.collectionView then
        self.collectionView.bindingSource = newSource
    end
    
    -- clear old items
    self:ClearCollection()
    -- update new items
    self.itemsSource = newSource

    if self.itemsSource ~= nil then
        self.itemsSource:subscribe(CollectionChanged,self.collectionChanged)
        self:CreateCollection()
    end
end

---@private
---@param sender any
---@param propertyName string
function DynamicCollectionBinding:OnPropertyChanged(sender, propertyName, oldValue, newValue)
    assert(self:IsBound(),"[CollectionBinding]Try to notify on property changed on a unbind binding")
    if sender ~= self.source then
        return
    end

    if propertyName ~= nil and self.sourcePropertyName ~= propertyName then
        -- ignore invalid propertyName
        return
    end

    if newValue == nil or newValue.__classname ~= "ObservableList" then
        self:ChangeItemsSource(nil)
        error("[CollectionBinding.Bind]Binding failed,collection object is nil or not a ObservableList type object")
        return
    end

    if newValue == self.itemsSource then
        -- same collectio no need to update
        return
    end

    self:ChangeItemsSource(newValue)
end

---@private
---@param sender any
---@param args {action,newItems,oldItems}
function DynamicCollectionBinding:OnCollectionChanged(sender, args)
    assert(args ~= nil,"[CollectionBinding.OnCollectionBinding] args is nil")
    self:RefreshView()
end
---@private
---@param index number
function DynamicCollectionBinding:BindItem(index)
    local source = self.itemsSource[index]
    local view = self.factory:CreateViewItem()
    
    -- set source
    local luaTable = LuaBindingTargetWrapper.GetLuaTable(view)
    ---@type DataContext
    local dataContext = luaTable["dataContext"]
    if dataContext == nil then
        error("[ObservableList.AddItems]The item prefab must add LuaBehaviour and added the data context in lua view script")
        return
    end
    dataContext:SetSource(source)
    return view
end

---@private
---@param item any
function DynamicCollectionBinding:UnbindItem(item)
    self.factory:ReleaseViewItem(item.view)
    -- clear source
    local luaTable = LuaBindingTargetWrapper.GetLuaTable(item.view)
    -- maybe view had been destroyed when exit application
    if luaTable == nil then
        return
    end
    
    local dataContext = luaTable["dataContext"]
    if dataContext == nil then
        error("[ObservableList.AddItems]The item prefab must add LuaBehaviour and added the data context in lua view script")
        return
    end
    dataContext:SetSource(nil)
end

---@param index number
---@param sortedIndex number
------@private
function DynamicCollectionBinding:AddNewItem(index, sortedIndex)
    if not self.factory.prefabPrepare then
        self.factory.onPrefabPrepared = function()  self:UpdateItems() end 
        return
    end
    
    local view = self:BindItem(index)
    -- add bound item
    self.bindingDictionary:Add(index,DynamicViewItem(index,view,sortedIndex))
end

---@private
function DynamicCollectionBinding:ClearItems()
    self.bindingDictionary:ForEach(function(key,value)
        self:UnbindItem(value)
    end)
    
    self.bindingDictionary:Clear()
end

---@private
function DynamicCollectionBinding:UpdateItems()
    if not self:IsBound() then
        return
    end
    
    local list = self.factory:GetDynamicList()
    -- check if items is valid or not
    self.bindingDictionary:ForEach(function(key,value)
        if not list:Contains(value.index) then
            self.pendingList:Add(value)
        end
    end)
    
    -- clear invalid items
    if self.pendingList:Count() > 0 then
        self.pendingList:ForEach(function(item)
            self:UnbindItem(item)
            -- remove items
            self.bindingDictionary:Remove(item.index)
        end)
        
        self.pendingList:Clear()
    end

    local sortedIndex = 0
     -- check missing items
     list:ForEach(function(index)
         sortedIndex = sortedIndex + 1
         local item = self.bindingDictionary[index]
         if item ~= nil then
             item.sortedIndex = sortedIndex
             return
         end
         
         -- create new item
         self:AddNewItem(index,sortedIndex)
     end)
end
---@private
function DynamicCollectionBinding:RefreshView()
    if self.collectionView ~= nil then
        self.collectionView:RefreshView()
    else
        self.factory:UpdateView()
    end
end

return DynamicCollectionBinding