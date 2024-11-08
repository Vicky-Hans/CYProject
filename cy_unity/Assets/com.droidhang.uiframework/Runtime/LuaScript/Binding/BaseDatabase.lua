---
--- 文件名称:  BaseDatabase
--- 创建者:    yuancan
--- 创建时间:  2022/2/8 3:00 PM
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")
--模块
---@class BaseDatabase
local BaseDatabase = bpcClass("BaseDatabase")
local ObservableObject = require("Binding/ObservableObject")

---@public
---@param collection ObservableDictionary
---@param data table[] | table<number|string,table>
---@param collectionChanged fun(args:NotifyDictionaryChangedEventArgs)
function BaseDatabase:SetCollection(collection,data,collectionChanged)
    collection:BindCollectionChanged(collectionChanged)
    for k, v in pairs(data) do
        collection:Add(k,self:SetProxy(v))
    end
end

---@public
---@param collection ObservableDictionary
---@param collectionChanged fun(args:NotifyDictionaryChangedEventArgs)
function BaseDatabase:ReleaseCollection(collection,collectionChanged)
    collection:Clear()
    collection:UnBindCollectionChanged(collectionChanged)
end

---@public
---@param obj table
---@return ObservableObject
function BaseDatabase:SetProxy(obj)
    return ObservableObject:SetProxy(obj)
end

return BaseDatabase