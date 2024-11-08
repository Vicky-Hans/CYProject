---
--- 文件名称:  List
--- 创建者:    yuancan.
--- 创建时间:  2021/7/15 9:37 AM
-------------------------------------------------------------------
--- 功能描述：
---
---

---@generic T
---@class List
local List = bpcClass("List")

---@return List
function List:New(t)--创建List对象
    self.count = 0
    local o = {itemType = t}
    self.__tostring = function(table)
        local str = "[List] Count %s Value -> {%s}"
        local dataStr = ""
        table:ForEach(function(item)
            dataStr = dataStr.."["..tostring(item).."]"
        end)

        return string.format(str,table:Count(),dataStr)
    end
    setmetatable(o, self)
    return o
end

---@return List
function List:Add(item)--添加元素
    table.insert(self, item)
    self.count = self.count + 1
    return self
end

---@return List
function List:AddRange(collection)
    if collection.Count == nil then
        error("Only support add List range,should use the class List:New() to create the collection")
    end
    
    collection:ForEach(function(item)
        self:Add(item)
    end)
    
    return self
end

function List:Clear()--清空
    local count = self:Count()
    for i=count,1,-1 do
        table.remove(self)
    end
    self.count = 0
end

function List:Contains(item)--是否包含某个元素
    local count = self:Count()
    for i=1,count do
        if self[i] == item then
            return true
        end
    end
    return false
end

function List:Count()--数量
    return self.count
end

function List:Find(predicate)--查找
    if (predicate == nil or type(predicate) ~= 'function') then
        print('predicate is invalid!')
        return
    end
    local count = self:Count()
    for i=1,count do
        if predicate(self[i]) then
            return self[i]
        end
    end
    return nil
end

---@alias Handler fun(item: any):void
---@param action Handler | "function(item) end"
function List:ForEach(action)--遍历，参数function
    if (action == nil or type(action) ~= 'function') then
        print('action is invalid!')
        return
    end
    local count = self:Count()
    for i=1,count do
        action(self[i])
    end
end

function List:IndexOf(item)--元素的在List中的索引
    local count = self:Count()
    for i=1,count do
        if self[i] == item then
            return i
        end
    end
    
    return 0
end

function List:IndexOfByPredicate(predicate)--查找元素的在List中的索引
    if (predicate == nil or type(predicate) ~= 'function') then
        print('predicate is invalid!')
    end
    
    local count = self:Count()
    for i=1,count do
        if predicate(self[i]) then
            return i
        end
    end
    
    return 0
end

function List:LastIndexOf(item)
    local count = self:Count()
    for i=count,1,-1 do
        if self[i] == item then
            return i
        end
    end
    return 0
end

function List:Insert(index, item)--插入
    table.insert(self, index, item)
    self.count = self.count + 1
    return self
end

function List:ItemType()
    return self.itemType
end

function List:Remove(item)--移除
    assert(self.count > 0,"[List.Remove] index out of range")
    local idx = self:LastIndexOf(item)
    if (idx > 0) then
        table.remove(self, idx)
    end
    self.count = self.count - 1
    return idx
end

function List:RemoveAt(index)--在某个位置移除
    assert(index > 0 and index <= self.count,"[List.RemoveAt] index out of range")
    local item = self[index]
    table.remove(self, index)
    self.count = self.count - 1
    return item
end

function List:RemoveLast()
    assert(self.count > 0,"[List.RemoveLast] index out of range")
    local index = self.count
    local item = self[index]
    table.remove(self, index)
    self.count = self.count - 1
    return item
end

function List:Sort(comparison)--排序
    if (comparison ~= nil and type(comparison) ~= 'function') then
        print('comparison is invalid')
        return
    end
    
    if comparison == nil then
        table.sort(self)
    else
        table.sort(self, comparison)
    end
end

return List