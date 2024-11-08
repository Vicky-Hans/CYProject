---
--- 文件名称:  ObservableList
--- 创建者:    yuancan.
--- 创建时间:  2021/7/16 2:04 PM
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")
local EventHandler = require("Support/EventHandler")
local List = require("Common/List")
local Dictionary = require("Common/Dictionary")

--模块
---@class ObservableList
local ObservableList = bpcClass("ObservableList")
-- property change name
local CountString = "count"
local IndexerEvent = "Item[]"
local CollectionChanged = "CollectionChanged"
local enableDebug = DataBindingDebug

--[[--
可观察对象的__newindex函数
]]
---@param t table
---@param k string
---@param v any
local __newindex = function(t,k,v)
    if type(v)=="function" or string.find(k, "^_") == 1 then
        error("[ObservableList]Not support modify Observable list function")
    else
        local list = t["list"]
        if list == nil then
            error("[ObservableList]unexpected error")
        end

        if type(k) ~= "number" then
            error("[ObservableList]Only support use number to access")
        end

        local old = list[k]
        if old == nil then
            error("[ObservableList]Need to add item first,current index with "..k.." not exist")
        end

        rawset(list,"_attr_"..k,v)

        local args =
        {
            action = t.NotifyCollectionChangeAction.Replace,
            oldItems = List:New():Add(old),
            newItems = List:New():Add(v),
            index = k,
            oldIndex = k,
        }
        t:OnCollectionChanged(args)
    end
end

--[[--
构造函数
@param #table self
@param #table t 初始化参数
]]
function ObservableList:ctor()
    ---@type List
    self.list = List:New()
    self.isNotifying = true
    ---@type Dictionary
    self.actions = Dictionary:New("string","EventHandler")
    self.NotifyCollectionChangeAction =
    {
        Add = 0,
        Remove = 1,
        Replace = 2,
        Insert = 3,
        Reset = 4,
    }
    
    -- 重载tostring函数，用于输出ObservableList详细信息
    local original_meta = getmetatable(self)
    local __index = function(t,k)
        if type(k) == "number" then
            return t.list[k]
        end

        if k == "count" then
            return t.list.count
        end
        
        if string.find(k, "^_") == 1 then
            return getmetatable(t)[k]
        else
            local v = rawget(t,"_attr_"..k)
            if v ~= nil then
                return v
            end
            return getmetatable(t)[k]
        end
    end
    local newTable = { __index = __index , __newindex = __newindex}
    newTable.__tostring = function(table)
        local str = "[ObservableList] Count %s Value -> {%s}"
        local dataStr = ""
        table.list:ForEach(function(item)
            dataStr = dataStr.."["..tostring(item).."]"
        end)
        
        return string.format(str,table.list:Count(),dataStr)
    end

    local meta = setmetatable(newTable, original_meta)
    setmetatable(self, meta)
end

---@param collection List
---@public
function ObservableList:AddRange(collection)
    if not collection:Count() then
        return
    end
    
    self.list:AddRange(collection)
    local args =
    {
        action = self.NotifyCollectionChangeAction.Add,
        newItems = List:New():AddRange(collection),
        index = self.list:Count()
    }
    local count = self.list:Count()
    local deltaCount = collection:Count()
    self:OnCollectionChanged(args,count-deltaCount,count)
end

---@param item any
---@public
function ObservableList:Add(item)
    self.list:Add(item)
    
    local args = 
    {
        action = self.NotifyCollectionChangeAction.Add,
        newItems = List:New():Add(item),
        index = self.list:Count()
    }
    
    local count = self.list:Count()
    self:OnCollectionChanged(args,count-1,count)
end

---@param item any
---@public
function ObservableList:Remove(item)
    local index = self.list:Remove(item)
    local args =
    {
        action = self.NotifyCollectionChangeAction.Remove,
        oldItems = List:New():Add(item),
        index = index
    }
    local count = self.list:Count()
    self:OnCollectionChanged(args,count+1,count)
end

---@param index number
---@return any
---@public
function ObservableList:RemoveAt(index)
    local item = self.list:RemoveAt(index)
    local args =
    {
        action = self.NotifyCollectionChangeAction.Remove,
        oldItems = List:New():Add(item),
        index = index
    }
    local count = self.list:Count()
    self:OnCollectionChanged(args,count+1,count)

    return item
end
---@public
function ObservableList:RemoveLast()
    self:RemoveAt(self.list:Count())
end

---@param index number
---@param collection List
---@public
function ObservableList:InsertRange(index, collection)
    if not collection:Count() then
        return
    end

    local targetIndex = index
    collection:ForEach(function(item)
        self.list:Insert(targetIndex, item)
        targetIndex = targetIndex + 1
    end)

    local args =
    {
        action = self.NotifyCollectionChangeAction.Insert,
        newItems = List:New():AddRange(collection),
        index = index
    }
    local count = self.list:Count()
    local deltaCount = collection:Count()
    self:OnCollectionChanged(args,count-deltaCount,count)
end

---@param index number
---@param item any
---@public
function ObservableList:Insert(index, item)
    self.list:Insert(index, item)
    
    local args = 
    {
        action = self.NotifyCollectionChangeAction.Insert,
        newItems = List:New():Add(item),
        index = index
    }
    
    local count = self.list:Count()
    self:OnCollectionChanged(args,count-1,count)
end

---@public
function ObservableList:Clear()
    local count = self.list:Count()
    self.list:Clear()
    local args =
    {
        action = self.NotifyCollectionChangeAction.Reset,
    }
    self:OnCollectionChanged(args, count, 0)
end
---@private
function ObservableList:OnCollectionChanged(args, oldCount, newCount)

    if enableDebug then
        if args.newItems ~= nil then
            bpcPrintf("[ObservableList.OnCollectionChanged] %s %s", tostring(args.action), args.newItems[1])
        elseif args.oldItems ~= nil then
            bpcPrintf("[ObservableList.OnCollectionChanged] %s %s", tostring(args.action), args.oldItems[1])
        end
    end
    
    if self.isNotifying and self.actions[CollectionChanged] ~= nil then
        self:raisePropertyChanged(CollectionChanged,args,nil,nil)
    end
    
    -- notify count changed
    if self.actions[CountString] ~= nil and oldCount ~= newCount then
        self:raisePropertyChanged(CountString,nil,oldCount,newCount)
    end
end
---@public
function ObservableList:Count()
    return self.list:Count()
end

--[[--
订阅
]]
---@public
---@param key string
---@param action function
function ObservableList:subscribe(key, action)
    if not self.actions then
        error("[ObservableList.subscribe] attribute actions is nil")
        return
    end

    if action == nil or key == nil then
        error("[ObservableList.subscribe] action or key is nil")
        return
    end

    if not self.actions:ContainsKey(key) then
        self.actions:Add(key,EventHandler())
    end
    
    self.actions[key]:AddListener(action)

    if enableDebug then
        bpcPrintf("[[ObservableList.subscribe]key %s action %s",key,self.actions:Count())
    end

end

--[[--
退订
]]
---@public
---@param key string
---@param action function
function ObservableList:unsubscribe(key, action)
    if not self.actions then
        return
    end

    if not self.actions:ContainsKey(key) then
        return
    end

    self.actions[key]:RemoveListener(action)

    if self.actions[key]:Count() == 0 then
        self.actions:Remove(key)
    end
end

--[[--
触发属性改变的通知
]]
---@private
---@param key string
---@param args {action:function,oldItems:any,newItems:any}
---@param old any
---@param new any
function ObservableList:raisePropertyChanged(key, args, old, new)
    if not self.actions then
        return
    end

    if enableDebug then
        bpcPrintf("[[ObservableList.raisePropertyChanged]key %s",key)
    end
    
    if self.actions and self.actions:ContainsKey(key) then
        local handler = self.actions[key]
        if old == nil then
            handler:Invoke(self,args)
        else
            handler:Invoke(self,key,old,new)   
        end
    end
end

---@alias Handler fun(item: any):void
---@param action Handler | "function(item) end"
function ObservableList:ForEach(action)
    assert(self.list ~= nil,"try to foreach a nil list")
    self.list:ForEach(action)
end

---@param predicate function
function ObservableList:Find(predicate)
    assert(self.list ~= nil,"try to find a nil list")
    return self.list:Find(predicate)
end

function ObservableList:Contains(item)--是否包含某个元素
    return self.list:Contains(item)
end

function ObservableList:IndexOf(item)
    return self.list:IndexOf(item)
end

function ObservableList:IndexOfByPredicate(predicate)
    return self.list:IndexOfByPredicate(predicate)
end

return ObservableList