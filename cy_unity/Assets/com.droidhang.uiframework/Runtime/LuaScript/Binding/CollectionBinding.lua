---
--- 文件名称:  CollectionBinding
--- 创建者:    yuancan.
--- 创建时间:  2021/7/16 1:59 PM
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")
local UDH = require("UChatGlobalRequire")
local LuaBindingTargetWrapper = CS.UIFramework.LuaBindingTargetWrapper
local Binding = require("Binding/Binding")
local CollectionView = require("CollectionView/CollectionView")
local Dictionary = require("Common/Dictionary")
local TextPathParser = require("Support/TextPathParser")
local ChainedSource = require("Binding/ChainedSource")
local enableDebug = DataBindingDebug
--模块
---@class CollectionBinding
local CollectionBinding = bpcClass("CollectionBinding")

local IndexerEvent = "Item[]"
local CollectionChanged = "CollectionChanged"
local OnPropertyChanged = "OnPropertyChanged"

--[[--
构造函数
@param #table self
@param #table t 初始化参数
]]
function CollectionBinding:ctor(t)
    self.source = nil
    self.sourcePath = nil
    self.sourcePropertyName = nil
    self.itemsSource = nil
    ---@type any
    self.factory = nil
    ---@type CollectionView
    self.collectionView = nil
    ---@type Dictionary
    self.bindingDictionary = Dictionary:New("object","GameObject")
    ---@type Dictionary
    self.viewReferenceCountDictionary = Dictionary:New("GameObject","int")
    ---@type function
    self.collectionChanged = function(sender,args) self:OnCollectionChanged(sender,args)  end
    self.propertyChanged = function(sender,propertyName) self:OnPropertyChanged(sender,propertyName) end
    self.notifyCollectionChangeAction =
    {
        Add = 0,
        Remove = 1,
        Replace = 2,
        Move = 3,
        Reset = 4,
    }
    -- init collection change callback handlers
    self.actionHandler = {}
    self.actionHandler[self.notifyCollectionChangeAction.Add] = function(args)
        self:AddItems(args)
        self:RefreshView()
    end
    self.actionHandler[self.notifyCollectionChangeAction.Remove] = function(args) 
        self:RemoveItems(args) 
        self:RefreshView()  
    end
    self.actionHandler[self.notifyCollectionChangeAction.Replace] = function(args) self:ReplaceItems(args)  end
    self.actionHandler[self.notifyCollectionChangeAction.Move] = function(args) self:MoveItems(args)  end
    self.actionHandler[self.notifyCollectionChangeAction.Reset] = function(args) self:ResetItems(args)  end
    
    -- collection view
    ---@type CollectionView
    self.collectionView = nil
    ---@type Binding
    self.collectionViewBinding = nil
end

---@public
---@return boolean
function CollectionBinding:IsBound()
    return self.itemsSource ~= nil
end
---@public
---@return CollectionBinding
function CollectionBinding:To(sourcePath, factory)
    if factory == nil then
        local DefaultViewFactory = require("Support/DefaultViewFactory")
        factory = DefaultViewFactory()
    end

    if sourcePath == nil then
        error("[CollectionBinding.To]source path is nil")
        return
    end

    if self.content == nil then
        error("[CollectionBinding.To]Should call Target first")
        return
    end
    
    self.factory = factory
    self.factory.content = self.content
    self.factory.viewTemplate = self.viewTemplate
    self.factory:PrepareTemplate()
    self.content = nil
    self.viewTemplate = nil
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
---@param content Transform
---@param viewTemplate GameObject
---@return CollectionBinding
function CollectionBinding:Target(content, viewTemplate)
    assert(content ~= nil,"[CollectionBinding]Target content is nil")
    assert(viewTemplate ~= nil,"[CollectionBinding]Target viewTemplate is nil")
    self.content = content
    self.viewTemplate = viewTemplate
    return self
end

---@public
---@param collectionViewPath string
---@return CollectionBinding
function CollectionBinding:WithCollectionView(collectionViewPath)
    self.collectionView = CollectionView(self.bindingDictionary)
    self.collectionViewBinding = Binding():Target(self):For("collectionView"):To(collectionViewPath):OneWayToSource()
    return self
end

---@public
---@param dataContext DataContext
---@return CollectionBinding
function CollectionBinding:Build(dataContext)
    assert(dataContext ~= nil,"[CollectionBinding]Build dataContenxt is nil")
    dataContext:AddBinding(self)
    if self.collectionViewBinding ~= nil then
        dataContext:AddBinding(self.collectionViewBinding)
    end
    return self
end

---@public
---@param source any
function CollectionBinding:Bind(source)
    if source == nil then
        error("[CollectionBinding.Bind]source is nil")
        return
    end

    if self:IsBound() then
        self:Unbind()
    end

    --- chained source binding
    if self.token.path:Count() > 1 then
        self.source = ChainedSource(source, self.token)
    else
        self.source = source
    end
    
    -- handle nested property
    local bindSource = self.source
    if bindSource == nil then
        error("[CollectionBinding.Bind]bind source is nil")
        return
    end
    
    self.source = bindSource;
    bindSource:subscribe(self.sourcePropertyName,self.propertyChanged)
    
    ---@type ObservableList
    local collectionObject = nil
    if self.source.__classname == "ChainedSource" then
        collectionObject = self.source:GetValue()
        
        if collectionObject == nil then
            return
        end
    else
        collectionObject = self.source[self.sourcePropertyName]
    end
    
    if collectionObject == nil then
        error("[CollectionBinding.Bind]Binding failed,collection object is nil or not a ObservableList type object")
        return
    end
    
    self.itemsSource = collectionObject;
    collectionObject:subscribe(CollectionChanged,self.collectionChanged)
    
    self:AddItems(collectionObject)
    self:RefreshView()
end

---@public
function CollectionBinding:Unbind()
    if not self:IsBound() then
        return
    end
    
    self.itemsSource:unsubscribe(CollectionChanged,self.collectionChanged)
    self.source:unsubscribe(self.sourcePropertyName,self.propertyChanged)
    
    self:ClearItems()
    self:RefreshView()
    
    self.source = nil
    self.itemsSource = nil

    if  not self.factory then
        self.factory:Release()
    end
end

---@public
---@param sender any
---@param propertyName string
function CollectionBinding:OnPropertyChanged(sender, propertyName)
    assert(self:IsBound(),"[CollectionBinding]Try to notify on property changed on a unbind binding")
    if sender ~= self.source then
        return
    end

    if propertyName ~= nil and self.sourcePropertyName ~= propertyName then
        -- ignore invalid propertyName
        return
    end
    
    local collectionObject = self.source[self.sourcePropertyName]
    if collectionObject == self.itemsSource then
        -- same collectio no need to update
        return
    end
    
    self:ChangeItemsSource(collectionObject)
    self:RefreshView()
end

---@public
---@param newSource any
function CollectionBinding:ChangeItemsSource(newSource)
    if self.itemsSource ~= nil then
        self.itemsSource:unsubscribe(self.sourcePropertyName,self.collectionChanged)
    end
    -- clear old items
    self:ClearItems()
    -- update new items
    self.itemsSource = newSource
    if self.itemsSource then
        self.itemsSource:subscribe(self.sourcePropertyName,self.collectionChanged)
        self:AddItems(newSource)
    end
end

---@public
---@param sender any
---@param args {action,newItems,oldItems}
function CollectionBinding:OnCollectionChanged(sender, args)
    assert(args ~= nil,"[CollectionBinding.OnCollectionBinding] args is nil")
    if enableDebug then
        bpcPrintf("[CollectionBinding.OnCollectionChanged] action is %s",args.action)
    end
    self.actionHandler[args.action](args)
end

---@private
function CollectionBinding:InitViewReferenceCount()
    self.bindingDictionary:ForEach(function(key,value)
        self.viewReferenceCountDictionary:Add(value,1)
    end)
end

---@private
---@param args {action,newItems,oldItems}
function CollectionBinding:AddItems(args)
    local collection = args.newItems
    if collection == nil then
        collection = args
    end
    
    if not self.factory.preparePrefab then
        self.factory.onPrefabPrepared = function()
            self:AddItems(self.itemsSource) 
            self:RefreshView()
        end
        return
    end

    collection:ForEach(function(item,value)
        local view = self.bindingDictionary[item]
        if view ~= nil then
            if self.viewReferenceCountDictionary == nil then
                self:InitViewReferenceCount()
            end

            self.viewReferenceCountDictionary[view] = self.viewReferenceCountDictionary[view] + 1
            return
        end
        
        -- create view from factory
        view = self.factory:CreateViewItem()
        
        local luaTable = LuaBindingTargetWrapper.GetLuaTable(view)
        local dataContext = luaTable["dataContext"]
        if dataContext == nil then
            error("[ObservableList.AddItems]The item prefab must add LuaBehaviour and added the data context in lua view script")
            return
        end
        
        -- set data context source
        -- dictionary use value as source,key as binding dictionary key 
        -- list use item as source
        dataContext:SetSource(value or item)
        self.bindingDictionary:Add(item,view)
        if self.viewReferenceCountDictionary ~= nil then
            self.viewReferenceCountDictionary:Add(view,1)
        end
    end) -- end collection foreach

    if self.viewReferenceCountDictionary ~= nil then
        assert(self.bindingDictionary:Count() == self.viewReferenceCountDictionary:Count(),"[CollectionBinding]AddItems unexpected error occur")
    end
end

---@private
---@param args {action,newItems,oldItems}
function CollectionBinding:RemoveItems(args)
    local collection = args.oldItems
    collection:ForEach(function(item)
        local view = self.bindingDictionary[item]
        if view == nil then
            error("[ObservableList.RemoveItems]Unkown item "..tostring(item))
            return
        end
        
        -- decrease view reference
        local removeFlag = true
        if self.viewReferenceCountDictionary ~= nil then
            local count = self.viewReferenceCountDictionary[view]
            if count == nil then
                error("[ObservableList.RemoveItems]Unkown item "..item)
                return
            end
            
            count = count - 1
            self.viewReferenceCountDictionary:Add(view, count)
            removeFlag = count <= 0
        end -- end of view reference count check

        if removeFlag then
            -- remove view 
            -- reset source
            local luaTable = LuaBindingTargetWrapper.GetLuaTable(view)
            local dataContext = luaTable["dataContext"]
            dataContext:SetSource(nil)
            -- release view
            self.factory:ReleaseViewItem(view)
            -- remove from binding Dictionary
            self.bindingDictionary:Remove(item)
            -- remove from view reference Dictionary
            if self.viewReferenceCountDictionary ~= nil then
                self.viewReferenceCountDictionary:Remove(view)
            end
        end -- end of view release
        
    end) -- end of collection foreach

    if self.viewReferenceCountDictionary ~= nil then
        assert(self.viewReferenceCountDictionary:Count() == self.bindingDictionary:Count(),"[CollectionBinding]RemoveItems unexpected error occur")
    end
end

---@private
function CollectionBinding:ClearItems()
    self.bindingDictionary:ForEach(function(key,value)
        local view = value
        -- clear source
        local luaTable = LuaBindingTargetWrapper.GetLuaTable(view)
        -- when luaTable is nil,ignore release
        -- luaTable maybe release by view script
        if luaTable ~= nil then
            local dataContext = luaTable["dataContext"]
            dataContext:SetSource(nil)
        end
        -- destroy item
        self.factory:ReleaseViewItem(view)
    end) -- end release view items
    
    -- clear binding dictionary
    self.bindingDictionary:Clear()
    
    -- clear view reference
    if self.viewReferenceCountDictionary ~= nil then
        self.viewReferenceCountDictionary:Clear()
    end
end

---@private
function CollectionBinding:ResetItems()
    self:ClearItems()
    self:AddItems(self.itemsSource)
end

---@private
---@param args {action,newItems,oldItems}
function CollectionBinding:ReplaceItems(args)
    self:RemoveItems(args.oldItems)
    self:AddItems(args.newItems)
    
    self:ResetViewOrder(self.itemsSource)
end

function CollectionBinding:MoveItems(args)
    self:AddItems(args.newItems)
    self:ResetViewOrder(self.itemsSource)
end

---@private
---@param collection List
function CollectionBinding:ResetViewOrder(collection)
    local index = 0
    collection:ForEach(function(item)
        local view = self.bindingDictionary[item]
        if view == nil then
            error("Unknown item "..tostring(item))
            return
        end

        LuaBindingTargetWrapper.SetSiblingIndex(view,index)
        index = index + 1
    end)
end

---@private
function CollectionBinding:CreateCollectionView()
    self.collectionView = CollectionView()
    self.collectionView:Init(self.bindingDictionary)
end

---@private
function CollectionBinding:RefreshView()
    if self.collectionView == nil then
        return
    end
    
    -- refresh collection view
    self.collectionView:RefreshView()
end

return CollectionBinding