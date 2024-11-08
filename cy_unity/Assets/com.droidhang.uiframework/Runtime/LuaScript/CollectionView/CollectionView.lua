---
--- 文件名称:  CollectionView
--- 创建者:    yuancan.
--- 创建时间:  2021/7/20 3:45 PM
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")
local List = require("Common/List")

--模块
---@class CollectionView
local CollectionView = bpcClass("CollectionView")

---构造函数
---@param binding Dictionary
function CollectionView:ctor(binding)
    assert(binding ~= nil,"[CollectionView.ctor] binding dictionary is nil")
    self.filter = nil
    self.sort = nil
    self.bindingDictionary = binding
    self.sortingList = List:New()
    self.count = 0
    self.viewChange = nil
    self.compareTo = function(lhs,rhs)
        local result = self.sort(lhs,rhs)
        return result
    end
end

---@public
function CollectionView:ApplyFilter()
    local index = 0
    self.bindingDictionary:ForEach(function(key,item)
        local include = true
        -- invoke filter
        if self.filter ~= nil then
            include = not self.filter(key)
        end
        
        item:SetActive(include)

        if include then
            index = index + 1
        end
    end)

    self.count = index
end

---@public
function CollectionView:ApplySort()
    if self.sort == nil then
        return
    end
    
    -- reset sort List
    self.sortingList:Clear()
    self.bindingDictionary:ForEach(function(key,item)
        self.sortingList:Add(key)
    end)
    -- sort List
    self.sortingList:Sort(self.compareTo)
    
    local index = 1
    self.sortingList:ForEach(function(item)
        -- get view and set the sibling index
        local view = self.bindingDictionary[item]
        view.transform:SetSiblingIndex(index)
        item.Index = index
        index = index + 1
    end)
    
    self.sortingList:Clear()
end

---@public
function CollectionView:RefreshView()
    self:ApplyFilter()
    self:ApplySort()

    if self.viewChange ~= nil and type(self.viewChange) == "function" then
        self.viewChange()
    end
end



return CollectionView