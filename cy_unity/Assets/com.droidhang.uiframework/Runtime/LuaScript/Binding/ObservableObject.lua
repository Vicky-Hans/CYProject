---
--- 文件名称:  ObservableObject
--- 创建者:    yuancan.
--- 创建时间:  2021/7/15 11:03 AM
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")
---
--模块
---@class ObservableObject
local ObservableObject = bpcClass("ObservableObject")
local enableDebug = DataBindingDebug

--[[--
可观察对象的__index函数
]]
---@param  t table
---@param k string
local __index = function(t, k)
    local value = nil

    if string.find(k, "^_") == 1 then
        value = getmetatable(t)[k]
    else
        local v = rawget(t, "_attr_" .. k)
        if v ~= nil then
            value = v
        else
            value = getmetatable(t)[k]
        end
    end

    local finalValue = value

    local getter = rawget(t, k .. "Getter")
    if getter ~= nil then
        finalValue = getter(value) or value
    end
    return finalValue
end

--[[--
可观察对象的__newindex函数
]]
---@param  t table
---@param k string
---@param v any
local __newindex = function(t, k, v)
    if string.find(k, "^_") == 1 then
        rawset(t, k, v)
    else
        local oldValue = rawget(t, "_attr_" .. k)
        local newValue = v
        --- value not changed ,ignore notify property change
        if oldValue == newValue then
            return
        end

        rawset(t, "_attr_" .. k, v)

        local metatable = getmetatable(t)
        metatable.raisePropertyChanged(t, k, oldValue, newValue)

        local watcher = t['watch']
        if watcher == nil then
            return
        end

        local propertyWatcher = watcher[k]
        if propertyWatcher == nil then
            return
        end

        propertyWatcher(t, oldValue, newValue)
    end
end

--[[--
初始化函数，设置一个可观察对象实例的原表
]]
---@param t table
local function init(t)
    t.__actions = {}
    local orginal_meta = getmetatable(t)
    local meta = setmetatable({ __index = __index, __newindex = __newindex }, orginal_meta)
    setmetatable(t, meta)
end

--[[--
构造函数
@param #table self
@param #table t 初始化参数
]]
---@param t table
function ObservableObject:ctor(t)
    init(self)
    
    self.observable = true
    self.disposed = false
end

function ObservableObject:Create(t)
    local newValue = ObservableObject(t)
    for k, v in pairs(t) do
        local valueType = type(v)
        if valueType == "function" then
            --- not support
        elseif valueType == "table" then
            newValue[k] = ObservableObject:Create(v)
        else
            newValue[k] = v
        end
    end

    return newValue
end

function ObservableObject:InternalRelease()
    if self.disposed then
        return
    end

    self.disposed = true
    for _, v in pairs(self) do
        local typeName = type(v)
        if typeName == "userdata" then
            if v.Release then
                v:Release()
            end
        elseif typeName == "table" then
            if v.__classname == "ObservableList" or v.__classname == "ObservableDictionary" then
                v:ForEach(function(item)
                    if type(item) == "table" and item.InternalRelease then
                        item:InternalRelease()
                    end
                end)
            end
        end
    end
end

--[[--
订阅
@param #table self
@param #string key 属性名称
@param #function action 订阅的委托函数
]]
---@param key string
---@param action fun(key:string,oldValue:any,newValue:any)
function ObservableObject:Subscribe(key, action)
    self:subscribe(key, action)
end

--[[--
订阅
@param #table self
@param #string key 属性名称
@param #function action 订阅的委托函数
]]
---@param key string
---@param action function
---@private
function ObservableObject:subscribe(key, action)
    if not self.__actions then
        self.__actions = {}
    end

    if key and action then
        table.insert(self.__actions, { key = key, action = action })
    end
end
--[[--
退订
@param #table self
@param #string key 属性名称
@param #function action 退订的委托函数
]]
---@param key string
---@param action function
---@public
function ObservableObject:Unsubscribe(key, action)
    self:unsubscribe(key, action)
end

--[[--
退订
@param #table self
@param #string key 属性名称
@param #function action 退订的委托函数
]]
---@param key string
---@param action function
---@private
function ObservableObject:unsubscribe(key, action)
    if not self.__actions then
        return
    end

    if key and action then
        for k, v in pairs(self.__actions) do
            if key == v.key and action == v.action then
                table.remove(self.__actions, k)
                return
            end
        end
    end
end

--[[--
触发属性改变的通知
@param #table self
@param #string key 属性名称
]]
---@param t table
---@param key string
---@param old any
---@param new any
---@public
function ObservableObject:RaisePropertyChanged(key)
    self:raisePropertyChanged(key, nil, nil)
end

---@public
--- 刷新所有属性
function ObservableObject:NotifyAllPropertyChanged()
    self:raisePropertyChanged(nil, nil, nil)
end

--[[--
统一函数命名规范，raisePropertyChanged不再供外部使用
触发属性改变的通知
@param #table self
@param #string key 属性名称
]]
---@param t table
---@param key string
---@param old any
---@param new any
---@private
function ObservableObject:raisePropertyChanged(key, old, new)
    if not self.__actions then
        return
    end

    if self.__actions then
        for _, v in pairs(self.__actions) do
            if key == nil or v.key == key then
                v.action(self, key, old, new)
            end
        end
    end
end

---@public
function ObservableObject:IsObservableObject()
    return true
end

return ObservableObject