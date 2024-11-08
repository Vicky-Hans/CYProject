---
--- 文件名称:  BaseServerDataObservable
--- 创建者:    nieshihai
--- 创建时间:  2021/12/30 16:50
-------------------------------------------------------------------
--- 功能描述：
---
---

local UDH = require("UChatGlobalRequire")
local ObservableObject = require("Binding/ObservableObject")
local DataContext = require("DataContext/DataContext")

---
--模块
---@class BaseServerDataObservable:ObservableObject
local BaseServerDataObservable = bpcClass("BaseServerDataObservable", ObservableObject)

---@param target ObservableObject
function BaseServerDataObservable:Watch(target)
    if not target:IsObservableObject() then
        error("需求观察的对象需要是ObservableObject对象，请检查"..target.__classname)
    end

    local dataContextKey = target.__classname.."DataContext"
    if self[dataContextKey] ~= nil then
        return
    end

    local bindFunc = self['Bind'..target.__classname]
    if bindFunc == nil then
        error('绑定无效，需要在'..self.__classname..'里定义函数： '..('Bind'..target.__classname))
    end

    local dataContext = DataContext()
    UDH.BindingManager:AddDataContext(dataContext, target.__classname)

    self[dataContextKey] = dataContext
    bindFunc(self, dataContext)
    dataContext:Build()
end

---@param target ObservableObject
function BaseServerDataObservable:UnWatch(target)
    local dataContextKey = target.__classname.."DataContext"
    local dataContext = self[dataContextKey]
    if dataContext == nil then
        return
    end

    -- 取消注册
    UDH.BindingManager:RemoveDataContext(dataContext)
    -- 释放对象
    self[dataContextKey] = nil
end

return BaseServerDataObservable