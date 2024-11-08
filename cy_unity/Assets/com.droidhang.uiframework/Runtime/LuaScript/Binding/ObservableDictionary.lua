---
--- 文件名称:  ObservableDictionary
--- 创建者:    yuancan
--- 创建时间:  2022/1/12 2:41 PM
-------------------------------------------------------------------
--- 功能描述：
---
---

---@class CollectionChangeAction
---@field Add number
---@field Remove number
---@field Replace number
---@field Insert number
---@field Reset number

---        Add = 0,
---        Remove = 1,
---        Replace = 2,
---        Insert = 3,
---        Reset = 4,
---@class NotifyDictionaryChangedEventArgs
---@field action CollectionChangeAction 
---@field oldItems Dictionary
---@field newItems Dictionary
---@field index any 
---@field oldIndex any

--模块
---@class ObservableDictionary
local ObservableDictionary = bpcClass("ObservableDictionary")
local Dictionary = require("Common/Dictionary")
local EventHandler = require("Support/EventHandler")
--[[--
构造函数
]]
-- property change name
local CountString = "count"
local IndexerEvent = "Item[]"
local CollectionChanged = "CollectionChanged"
local enableDebug = DataBindingDebug

--[[--
可观察对象的__newindex函数
]]
---@param t ObservableDictionary
---@param k string
---@param v any
local __newindex = function(t,k,v)
    if type(v)=="function" or string.find(k, "^_") == 1 then
        error("[ObservableDictionary]Not support modify Observable dictionary function")
    else
        local list = rawget(t,"list")
        if list == nil then
            error("[ObservableDictionary]unexpected error")
        end

        local old = list[k]
        if old == nil then
            error("[ObservableDictionary]Need to add item first,current index with "..k.." not exist")
        end

        list:Add(k, v)

        ---@type NotifyDictionaryChangedEventArgs
        local args =
        {
            action = t.NotifyCollectionChangeAction.Replace,
            oldItems = Dictionary:New(),
            newItems = Dictionary:New(),
            index = k,
            oldIndex = k,
        }
        args.oldItems:Add(k,old)
        args.newItems:Add(k,v)
        t:OnCollectionChanged(args)
    end
end

--[[--
构造函数
@param #table self
@param #table t 初始化参数
]]
function ObservableDictionary:ctor()
    ---@type Dictionary
    self.list = Dictionary:New()
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

    -- 重载tostring函数，用于输出ObservableDictionary详细信息
    local original_meta = getmetatable(self)
    local __index = function(t,k)
        local func = ObservableDictionary[k]
        if func ~= nil then
            return func
        end
        
        local dataTable = rawget(t,"list")
        local result = dataTable[k]
        if result ~= nil then
            return result
        end
        
        return rawget(t,k)
    end
    local newTable = { __index = __index , __newindex = __newindex}
    ---@param table ObservableDictionary
    newTable.__tostring = function(table)
        local str = "[ObservableDictionary] Count %s Value -> {%s}"
        local dataStr = ""
        table.list:ForEach(function(key,value)
            dataStr = dataStr.."["..tostring(key)..tostring(value).."]"
        end)

        return string.format(str,table.list:Count(),dataStr)
    end

    local meta = setmetatable(newTable, original_meta)
    setmetatable(self, meta)
end

---@param collection Dictionary | table<any, any>
---@public
function ObservableDictionary:AddRange(collection)
    if not ((collection.__classname == nil and type(collection) == "table") or collection.__classname == "Dictionary") then
        return
    end

    local deltaCount = self.list:AddRange(collection)

    if deltaCount == 0 then
        return
    end
    
    ---@type NotifyDictionaryChangedEventArgs
    local args =
    {
        action = self.NotifyCollectionChangeAction.Add,
        newItems = Dictionary:New(),
        index = self.list:Count()
    }
    args.newItems:AddRange(collection)
    
    local count = self.list:Count()
    self:OnCollectionChanged(args,count-deltaCount,count)
end

---@param key any
---@param value any
---@public
function ObservableDictionary:Add(key,value)
    self.list:Add(key,value)

    ---@type NotifyDictionaryChangedEventArgs
    local args =
    {
        action = self.NotifyCollectionChangeAction.Add,
        newItems = Dictionary:New(),
        index = self.list:Count()
    }
    args.newItems:Add(key,value)

    local count = self.list:Count()
    self:OnCollectionChanged(args,count-1,count)
end

function ObservableDictionary:Keys()
    return self.list:Keys()
end

---@param key any
---@public
function ObservableDictionary:Remove(key)
    local value = self.list[key]
    local index = self.list:Remove(key)
    ---@type NotifyDictionaryChangedEventArgs
    local args =
    {
        action = self.NotifyCollectionChangeAction.Remove,
        oldItems = Dictionary:New(),
        index = index
    }
    args.oldItems:Add(key,value)
    local count = self.list:Count()
    self:OnCollectionChanged(args,count+1,count)
end

---@public
function ObservableDictionary:Clear()
    self.list:Clear()
    local args =
    {
        action = self.NotifyCollectionChangeAction.Reset,
    }
    self:OnCollectionChanged(args)
end

---@public
---@param callback fun(sender:ObservableDictionary,args:NotifyDictionaryChangedEventArgs)
function ObservableDictionary:BindCollectionChanged(callback)
   self:Subscribe(CollectionChanged,callback) 
end

---@public
---@param callback fun(sender:ObservableDictionary,args:NotifyDictionaryChangedEventArgs)
function ObservableDictionary:UnBindCollectionChanged(callback)
    self:UnSubscribe(CollectionChanged,callback)
end

---@private
---@param args NotifyDictionaryChangedEventArgs
function ObservableDictionary:OnCollectionChanged(args, oldCount, newCount)
    if enableDebug then
        if args.newItems ~= nil then
            bpcPrintf("[ObservableDictionary.OnCollectionChanged] %s %s", tostring(args.action), args.newItems[1])
        elseif args.oldItems ~= nil then
            bpcPrintf("[ObservableDictionary.OnCollectionChanged] %s %s", tostring(args.action), args.oldItems[1])
        end
    end

    if self.isNotifying and self.actions[CollectionChanged] ~= nil then
        self:RaisePropertyChanged(CollectionChanged,args,nil,nil)
    end

    -- notify count changed
    if self.actions[CountString] ~= nil and oldCount ~= newCount then
        self:RaisePropertyChanged(CountString,nil,oldCount,newCount)
    end
end



---@public
function ObservableDictionary:Count()
    return self.list:Count()
end

--[[--
订阅
]]
---@public
---@param key string
---@param action function
function ObservableDictionary:Subscribe(key, action)
    self:subscribe(key,action)
end

--[[--
订阅
]]
---@private
---@param key string
---@param action function
function ObservableDictionary:subscribe(key, action)
    if not self.actions then
        error("[ObservableDictionary.subscribe] attribute actions is nil")
        return
    end

    if action == nil or key == nil then
        error("[ObservableDictionary.subscribe] action or key is nil")
        return
    end

    if not self.actions:ContainsKey(key) then
        self.actions:Add(key,EventHandler())
    end

    self.actions[key]:AddListener(action)

    if enableDebug then
        bpcPrintf("[[ObservableDictionary.subscribe]key %s action %s",key,self.actions:Count())
    end

end

--[[--
退订
]]
---@public
---@param key string
---@param action function
function ObservableDictionary:UnSubscribe(key, action)
    self:unsubscribe(key,action)
end

--[[--
退订
]]
---@private
---@param key string
---@param action function
function ObservableDictionary:unsubscribe(key, action)
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
---@param args {action,oldItems,newItems}
---@param old any
---@param new any
function ObservableDictionary:RaisePropertyChanged(key, args, old, new)
    if not self.actions then
        return
    end

    if enableDebug then
        bpcPrintf("[[ObservableDictionary.raisePropertyChanged]key %s",key)
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

---@generic T, K
---@param action fun(key:T,value:K)
function ObservableDictionary:ForEach(action)
    assert(self.list ~= nil,"try to foreach a nil list")
    self.list:ForEach(action)
end

---@param predicate function
function ObservableDictionary:Find(predicate)
    assert(self.list ~= nil,"try to find a nil list")
    return self.list:Find(predicate)
end

function ObservableDictionary:ContainsKey(key)--是否包含某个Key
    return self.list:ContainsKey(key)
end

function ObservableDictionary:ContainsValue(item)--是否包含某个Item
    return self.list:ContainsValue(item)
end

return ObservableDictionary