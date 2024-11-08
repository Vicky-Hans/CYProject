---
--- 文件名称:  CircularDynamicCollectionBinding
--- 创建者:    nieshihai
--- 创建时间:  2021/9/16 14:02
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")
local UDH = require("UChatGlobalRequire")
local LuaBindingTargetWrapper = CS.UIFramework.LuaBindingTargetWrapper
local DynamicCollectionView = require("CollectionView/DynamicCollectionView")
local List = require("Common/List")
local Dictionary = require("Common/Dictionary")
local CollectionChanged = "CollectionChanged"
local ChainedSource = require("Binding/ChainedSource")
local TextPathParser = require("Support/TextPathParser")
local ObservableList = require("Binding/ObservableList")

---
--模块
---@class CircularDynamicCollectionBinding
local CircularDynamicCollectionBinding = bpcClass("CircularDynamicCollectionBinding")

--[[--
构造函数
@param #table self
@param #table t 初始化参数
]]
function CircularDynamicCollectionBinding:ctor(t)
    ---@type any
    self.source = nil
    ---@type string
    self.sourcePath = nil
    ---@type ObservableList|ObservableDictionary
    self.itemsSource = nil
    self.itemsSourceKeys = nil

    ---@type CircularDynamicViewFactory
    self.factory = nil
    ---@type List
    self.pendingList = List:New()
    ---@type Dictionary<number,DynamicViewItem>
    self.bindingDictionary = Dictionary:New("int", "Item")
    self.collectionChanged = function(sender, args)
        self:OnCollectionChanged(sender, args)
    end

    self.propertyChanged = function(sender, propertyName, oldValue, newValue)
        self:OnPropertyChanged(sender, propertyName, oldValue, newValue)
    end

    self.notifyCollectionChangeAction = {
        Add = 0,
        Remove = 1,
        Replace = 2,
        Insert = 3,
        Reset = 4,
    }
    -- init collection change callback handlers
    self.actionHandler = {}
    self.actionHandler[self.notifyCollectionChangeAction.Add] = function(args)
        if self:IsSourceObservableDiction() then
            args.newItems:ForEach(function(key, item)
                self.itemsSourceKeys:Add(key)
            end)
        end

        self:AddItem(args.index)
    end
    self.actionHandler[self.notifyCollectionChangeAction.Remove] = function(args)
        self:RefreshView(false)
    end
    self.actionHandler[self.notifyCollectionChangeAction.Replace] = function(args)
        self:RefreshView(false)
    end
    self.actionHandler[self.notifyCollectionChangeAction.Insert] = function(args)
        self:InsertItems(args.index, args.newItems:Count())
    end
    self.actionHandler[self.notifyCollectionChangeAction.Reset] = function(args)
        self:RefreshView(false)
    end
end

---@public
function CircularDynamicCollectionBinding:BindToProperty()
    return self.sourcePath ~= nil
end

---@public
---@return number
function CircularDynamicCollectionBinding:Count()
    if self.collectionView ~= nil then
        return self.collectionView.indexedList:Count()
    end
    return self.itemsSource:Count()
end

---@public
---@return boolean
function CircularDynamicCollectionBinding:IsBound()
    return self.source ~= nil
end

---@return CircularDynamicCollectionBinding
---@param sourcePath string
function CircularDynamicCollectionBinding:To(sourcePath)
    if sourcePath == nil then
        error("[CollectionBinding.To]source path is nil")
        return
    end

    self.sourcePath = sourcePath
    self.sourcePropertyName = UDH.BindingManager:GetPropertyPath(sourcePath)

    ---@type TextPathParser
    local parser = TextPathParser(sourcePath)
    ---@type Path
    local path = parser:Parse()
    ---@type PathToken
    self.token = path:AsPathToken()

    return self
end

---@public
---@param viewModeVM string
---@return CircularDynamicCollectionBinding
function CircularDynamicCollectionBinding:WithCollectionView(viewModeVM)
    ---@type DynamicCollectionView
    self.collectionView = DynamicCollectionView(self.itemsSource, self, viewModeVM)
    self.collectionView.viewChange = function(filterChanged)
        self.factory:UpdateView(filterChanged)
    end

    return self
end

---@return CircularDynamicCollectionBinding
---@param factory CircularDynamicViewFactory|CircularExpandTipFactory|CircularGroupFactory
function CircularDynamicCollectionBinding:Target(factory)
    self.factory = factory
    self.factory:Init(self)
    self.factory:PrepareTemplate()
    return self
end

---@param dataContext DataContext
---@return CircularDynamicCollectionBinding
function CircularDynamicCollectionBinding:Build(dataContext)
    assert(dataContext ~= nil, "[CollectionBinding]Build dataContenxt is nil")
    if self.collectionView ~= nil then
        self.collectionView:Build(dataContext)
    end
    dataContext:AddBinding(self)
    return self
end

---@public
---@param source ObservableObject
function CircularDynamicCollectionBinding:Bind(source)
    if self:IsBound() then
        self:Unbind()
    end

    --- chained source binding
    if self.token.path:Count() > 1 then
        self.source = ChainedSource(source, self.token)
    else
        self.source = source
    end

    if self:BindToProperty() then
        self.source:subscribe(self.sourcePropertyName, self.propertyChanged)
    end

    ---@type ObservableList
    local collectionObject = nil

    if self.source.__classname == "ChainedSource" then
        collectionObject = self.source:GetValue()
        --- ChainedSource allow null collection binding
        if collectionObject == nil then
            return
        end
    else
        collectionObject = self.source[self.sourcePropertyName]
    end

    if collectionObject == nil or (collectionObject.__classname ~= "ObservableList" and collectionObject.__classname ~= "ObservableDictionary") then
        error("[CollectionBinding.Bind]Binding failed,collection object is nil or is not a collection type")
    end

    self.itemsSource = collectionObject;

    if self:IsSourceObservableDiction() then
        if self.itemsSourceKeys == nil then
            self.itemsSourceKeys = List()
        else
            self.itemsSourceKeys:Clear()
        end
    end

    if self.collectionView then
        self.collectionView.bindingSource = collectionObject
    end

    collectionObject:subscribe(CollectionChanged, self.collectionChanged)

    self:CreateCollection()
end

---@public
function CircularDynamicCollectionBinding:Unbind()
    if not self:IsBound() then
        return
    end

    if self.itemsSource ~= nil then
        self.itemsSource:unsubscribe(CollectionChanged, self.collectionChanged)
    end

    if self:BindToProperty() and self.source ~= nil then
        self.source:unsubscribe(self.sourcePropertyName, self.propertyChanged)
    end

    self:ClearCollection()
    --clear source
    self.source = nil
    self.itemsSource = nil
    self.itemsSourceKeys = nil
end

---@private
function CircularDynamicCollectionBinding:CreateCollection()
    self:RefreshView(true)
end

---@private
function CircularDynamicCollectionBinding:ClearCollection()
    self.factory:ClearList()
end

---@private
---@param newSource any
function CircularDynamicCollectionBinding:ChangeItemsSource(newSource)
    if self.itemsSource ~= nil then
        self.itemsSource:unsubscribe(CollectionChanged, self.collectionChanged)
    end

    if self.collectionView then
        self.collectionView.bindingSource = newSource
    end

    -- update new items
    self.itemsSource = newSource

    if self:IsSourceObservableDiction() then
        if self.itemsSourceKeys == nil then
            self.itemsSourceKeys = ObservableList()
        else
            self.itemsSourceKeys:Clear()
        end
    else
        self.itemsSourceKeys = nil
    end

    if self.itemsSource ~= nil then
        self.itemsSource:subscribe(CollectionChanged, self.collectionChanged)
        self:CreateCollection()
    end
end

---@private
---@param sender any
---@param propertyName string
function CircularDynamicCollectionBinding:OnPropertyChanged(sender, propertyName, oldValue, newValue)
    assert(self:IsBound(), "[CollectionBinding]Try to notify on property changed on a unbind binding")
    if sender ~= self.source then
        return
    end

    if self.source.__classname == "ChainedSource" then
        newValue = self.source:GetValue()
    end

    if propertyName ~= nil and self.sourcePropertyName ~= propertyName then
        -- ignore invalid propertyName
        return
    end

    if newValue == nil or newValue.__classname ~= "ObservableList" then
        self:ChangeItemsSource(nil)
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
---@param args fun(action:table,newItems:table,oldItems:table)
function CircularDynamicCollectionBinding:OnCollectionChanged(sender, args)
    assert(args ~= nil, "[CollectionBinding.OnCollectionBinding] args is nil")

    if self.collectionView ~= nil then
        self:RefreshView(false)
    else
        self.actionHandler[args.action](args)
    end
end

---@public
---@return number
function CircularDynamicCollectionBinding:GroupCount()
    if self.collectionView ~= nil then
        return self.collectionView.groupItemList:Count()
    end

    return 0
end

---@public
---@return number
function CircularDynamicCollectionBinding:GroupForEach(action)
    if self.collectionView ~= nil then
        return self.collectionView.groupItemList:ForEach(action)
    end
end

function CircularDynamicCollectionBinding:GetItemIndexInGroup(groupIndex, itemIndexInGroup)
    if self.collectionView ~= nil then
        local groupItem = self.collectionView.groupItemList[groupIndex]

        if groupItem ~= nil then
            return groupItem.indexedList[itemIndexInGroup]
        end
    end

    return 0
end

---@private
---@param view GameObject
---@param index number
function CircularDynamicCollectionBinding:BindGroupButtonItem(view, index)
    if self.collectionView ~= nil then
        ---@type CircularDynamicGroupItem
        local groupItem = self.collectionView.groupItemList[index]

        if groupItem == nil then
            return
        end

        -- set source
        local luaTable = LuaBindingTargetWrapper.GetLuaTable(view)

        if luaTable == nil then
            return
        end

        ---@type DataContext
        local dataContext = luaTable["dataContext"]
        if dataContext == nil then
            error("[ObservableList.AddItems]The item prefab must add LuaBehaviour and added the data context in lua view script")
            return
        end

        dataContext:SetSource(groupItem.groupSource)
    end

    return view
end

---@private
---@param view GameObject
---@param index number
function CircularDynamicCollectionBinding:BindItem(view, index)
    local sourceIndex = index

    if self.collectionView ~= nil then
        sourceIndex = self.collectionView.indexedList[index]
    elseif self.itemsSourceKeys ~= nil then
        sourceIndex = self.itemsSourceKeys[sourceIndex]
    end

    local source = self.itemsSource[sourceIndex]

    -- set source
    local luaTable = LuaBindingTargetWrapper.GetLuaTable(view)
    ---@type DataContext
    local dataContext = luaTable["dataContext"]
    if dataContext == nil then
        error("[ObservableList.AddItems]The item prefab must add LuaBehaviour and added the data context in lua view script->%s", view.name)
        return
    end

    if source.OnBindCallback ~= nil then
        source:OnBindCallback(sourceIndex)
    end

    dataContext:SetSource(source)
    return view, source
end

---@public
---@param index number
function CircularDynamicCollectionBinding:GetItemSource(index)
    if self.collectionView ~= nil then
        index = self.collectionView.indexedList[index]
    elseif self.itemsSourceKeys ~= nil then
        index = self.itemsSourceKeys[index]
    end

    return self.itemsSource[index]
end

---@public
---@param index number
---@param isHeight boolean @是否获取的是高度
function CircularDynamicCollectionBinding:GetItemSize(index, isHeight)
    if self.collectionView ~= nil then
        index = self.collectionView.indexedList[index]
    elseif self.itemsSourceKeys ~= nil then
        index = self.itemsSourceKeys[index]
    end

    local source = self.itemsSource[index]
    local size = -1

    if source.CalculateItemSize ~= nil then
        source:CalculateItemSize()
    end

    if isHeight and source.ItemHeight ~= nil then
        size = source.ItemHeight
    elseif not isHeight and source.ItemWidth ~= nil then
        size = source.ItemWidth
    end

    return size, source;
end

---@private
---@param item GameObject
function CircularDynamicCollectionBinding:UnbindItem(item)
    -- clear source
    local luaTable = LuaBindingTargetWrapper.GetLuaTable(item)
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

---@private
function CircularDynamicCollectionBinding:RefreshView(jumpStart)
    if self.collectionView ~= nil then
        self.collectionView:RefreshView(jumpStart)
    else
        if self:IsSourceObservableDiction() then
            self.itemsSourceKeys:Clear()
            self.itemsSourceKeys:AddRange(self.itemsSource:Keys())
        end

        self.factory:UpdateView(jumpStart)
    end
end

---@private
function CircularDynamicCollectionBinding:IsSourceObservableDiction()
    return self.itemsSource ~= nil and self.itemsSource.__classname == "ObservableDictionary"
end

---@private
function CircularDynamicCollectionBinding:AddItem(totalCount)
    if self.collectionView ~= nil then
        self.collectionView:RefreshView(false)
    else
        self.factory:AddItems(totalCount)
    end
end

---@private
---@param insertIdx number @table的索引，从1开始
function CircularDynamicCollectionBinding:InsertItems(insertIdx, addCount)
    if self.collectionView ~= nil then
        self.collectionView:RefreshView(false)
    else
        self.factory:InsertItems(insertIdx - 1, addCount)
    end

    --- 监听的消息往后顺延插入的个数
    local checkIndex = insertIdx + addCount
    local tmpIdx = 1

    self.itemsSource:ForEach(function(item)
        if tmpIdx >= checkIndex then
            if item.circularItemIdx ~= nil then
                item.circularItemIdx = item.circularItemIdx + addCount
            end
        end
        tmpIdx = tmpIdx + 1
    end)
end

return CircularDynamicCollectionBinding