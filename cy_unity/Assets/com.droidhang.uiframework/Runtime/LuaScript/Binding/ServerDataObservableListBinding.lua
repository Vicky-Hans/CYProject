---
--- 文件名称:  ServerDataObservableListBinding
--- 创建者:    nieshihai
--- 创建时间:  2021/10/13 09:59
-------------------------------------------------------------------
--- 功能描述：
---
---

local List = require("Common/List")
local BindingManager = require("Binding/BindingManager")

--模块
---@class ServerDataObservableListBinding
local ServerDataObservableListBinding = bpcClass("ServerDataObservableListBinding")
local ObservableList = require("Binding/ObservableList")

local CollectionChanged = "CollectionChanged"

--[[--
构造函数
]]
---@param dataContext DataContext
function ServerDataObservableListBinding:ctor(dataContext)
    assert(dataContext ~= nil, "[ServerDataObservableListBinding]Build dataContext is nil")
    self.dataContext = dataContext
    self.source = nil
    self.sourcePath = nil
    self.sourcePropertyName = nil
    ---@type ObservableList
    self.itemsSource = nil
    ---@type function
    self.collectionChanged = function(sender,args) self:OnCollectionChanged(sender,args)  end
    self.propertyChanged = function(sender,propertyName) self:OnPropertyChanged(sender,propertyName) end

    self.cacheList = List:New("ProtoDataModel")
    
    self.AddCallback = nil
    self.RemoveCallback = nil
    self.UpdateCallback = nil
    self.ResetCallback = nil

    self.notifyCollectionChangeAction =
    {
        Add = 0,
        Remove = 1,
        Update = 2,
        Move = 3,
        Reset = 4,
    }
    
    -- init collection change callback handlers
    self.actionHandler = {}
    
    self.actionHandler[self.notifyCollectionChangeAction.Add] = function(args)
        if args.newItems ~= nil then
            self.cacheList:Clear()

            ---@param serverData ProtoDataModel
            args.newItems:ForEach(function(serverData)
                local dataWrap = serverData:Wrap()
                
                self.targetList:Add(dataWrap)
                self.cacheList:Add(dataWrap)
            end)

            if self.AddCallback ~= nil then
                self.AddCallback(self.cacheList)
            end

            self.cacheList:Clear()
        end
    end

    self.actionHandler[self.notifyCollectionChangeAction.Remove] = function(args)
        if args.oldItems ~= nil then
            self.cacheList:Clear()

            ---@param serverData ProtoDataModel
            args.oldItems:ForEach(function(serverData)
                local tmpIdx = self.targetList:IndexOfByPredicate(function(item)
                    return item.data == serverData
                end)

                if tmpIdx > 0 then
                    self.cacheList:Add(self.targetList[tmpIdx].data)
                    self.targetList:RemoveAt(tmpIdx)
                end
    
                if self.RemoveCallback ~= nil then
                    self.RemoveCallback(self.cacheList)
                end
    
                self.cacheList:Clear()
            end)
        end
    end
    
    self.actionHandler[self.notifyCollectionChangeAction.Update] = function(args)
        if self.UpdateCallback ~= nil and args.oldItems ~= nil and args.newItems ~= nil then
            local oldItem = args.oldItems[1]
            local newItem = args.newItems[1]

            local tmpIdx = self.targetList:IndexOfByPredicate(function(item)
                return item.data == oldItem
            end)

            if tmpIdx > 0 then
                self.targetList[tmpIdx].data = newItem
            end
            
            self.UpdateCallback(newItem, oldItem, tmpIdx)
        end
    end
    
    self.actionHandler[self.notifyCollectionChangeAction.Reset] = function(args)
        if self.targetList ~= nil then
            self.targetList:Clear()
        end
        
        if self.ResetCallback ~= nil then
            self.ResetCallback()
        end
    end
end

---@public
---@return boolean
function ServerDataObservableListBinding:IsBound()
    return self.itemsSource ~= nil
end

---@public
--- 绑定到VM里的一个ObservableList，可以做一些能用的处理
---@return ServerDataObservableListBinding
function ServerDataObservableListBinding:For(targetList)
    if targetList == nil then
        error("[ServerDataObservableListBinding.For]targetList is nil")
    end

    if targetList.__classname ~= "ObservableList" then
        error("[ServerDataObservableListBinding.For]targetList is not ObservableList")
    end

    self.targetList = targetList
    
    return self
end

---@public
---@return ServerDataObservableListBinding
function ServerDataObservableListBinding:To(sourcePath)
    if sourcePath == nil then
        error("[ServerDataObservableListBinding.To]source path is nil")
    end
    
    self.sourcePath = sourcePath
    self.sourcePropertyName = BindingManager:GetPropertyPath(sourcePath)
    
    return self
end

---@param addCallback function
---@param removeCallback function
---@param resetCallback function
---@param updateCallback function
function ServerDataObservableListBinding:OnCallback(addCallback, removeCallback, resetCallback, updateCallback)
    self.AddCallback = addCallback
    self.RemoveCallback = removeCallback
    self.ResetCallback = resetCallback
    self.UpdateCallback = updateCallback

    self.dataContext:AddBinding(self)
end

---@public
---@param dataContext DataContext
---@return CollectionBinding
function ServerDataObservableListBinding:Build(dataContext)
    assert(dataContext ~= nil,"[CollectionBinding]Build dataContenxt is nil")
    dataContext:AddBinding(self)
    return self
end

---@public
---@param source any
function ServerDataObservableListBinding:Bind(source)
    if source == nil then
        error("[CollectionBinding.Bind]source is nil")
        return
    end

    if self:IsBound() then
        self:Unbind()
    end

    -- handle nested property
    local bindSource = source
    if bindSource == nil then
        error("[CollectionBinding.Bind]bind source is nil")
        return
    end

    --@type ObservableList
    local collectionObject = bindSource[self.sourcePropertyName]
    if collectionObject == nil or collectionObject.__classname ~= "ObservableList" then
        error("[CollectionBinding.Bind]Binding failed,collection object is nil or not a ObservableList type object")
    end

    self.source = bindSource;
    self.itemsSource = collectionObject;

    collectionObject:subscribe(CollectionChanged,self.collectionChanged)
    bindSource:subscribe(self.sourcePropertyName,self.propertyChanged)
    self:InitValue()
end

---@public
function ServerDataObservableListBinding:Unbind()
    if not self:IsBound() then
        return
    end

    self.itemsSource:unsubscribe(CollectionChanged,self.collectionChanged)
    self.source:unsubscribe(self.sourcePropertyName,self.propertyChanged)

    self.source = nil
    self.itemsSource = nil
end

---@public
---@param sender any
---@param propertyName string
function ServerDataObservableListBinding:OnPropertyChanged(sender, propertyName)
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

    self:ChangeItemsSource(collectionObject)
end

---@public
---@param newSource any
function ServerDataObservableListBinding:ChangeItemsSource(newSource)
    if self.itemsSource ~= nil then
        self.itemsSource:unsubscribe(self.sourcePropertyName,self.collectionChanged)
    end

    self.actionHandler[self.notifyCollectionChangeAction.Reset](nil) -- 清除现有的列表内容
    
    self.itemsSource = newSource
    self.itemsSource:subscribe(self.sourcePropertyName,self.collectionChanged)
    self:InitValue()
end

---@public
---@param sender any
---@param args {action,newItems,oldItems}
function ServerDataObservableListBinding:OnCollectionChanged(sender, args)
    assert(args ~= nil,"[CollectionBinding.OnCollectionBinding] args is nil")

    if self.targetList == nil then
        self.targetList = ObservableList()
    end

    self.actionHandler[args.action](args)
end

function ServerDataObservableListBinding:InitValue()
    if not self:IsBound() then
        return
    end

    if self.itemsSource:Count() == 0 then
        return
    end

    local args =
    {
        newItems = List:New():AddRange(self.itemsSource.list),
    }
    
    self.actionHandler[self.notifyCollectionChangeAction.Add](args)
end

return ServerDataObservableListBinding