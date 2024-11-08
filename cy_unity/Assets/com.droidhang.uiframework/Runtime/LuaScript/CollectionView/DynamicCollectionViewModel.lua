---
--- 文件名称:  DynamicCollectionViewModel
--- 创建者:    nieshihai
--- 创建时间:  2022/2/11 18:13
-------------------------------------------------------------------
--- 功能描述：
--- 用于DynamicCollectionView的绑定
---

local ObservableObject = require("Binding/ObservableObject")

---
--模块
---@class DynamicCollectionViewModel:ObservableObject
---@field filter function @过滤的函数
---@field sort function @排序的函数
---@field group function @分组的函数
---@field dirty boolean
---@field init boolean @是否初始化完成
local DynamicCollectionViewModel = bpcClass("DynamicCollectionViewModel", ObservableObject)

function DynamicCollectionViewModel:ctor(filter, sort, group)
    DynamicCollectionViewModel.super.ctor(self)
    
    self.filter = filter
    self.sort = sort
    self.group = group
    self.dirty = false
    self.init = true
end

function DynamicCollectionViewModel:Dirty()
    self.dirty = true
end

DynamicCollectionViewModel.watch = {
    ---@param owner DynamicCollectionViewModel
    ---@param oldVal function
    ---@param newVal function
    filter = function(owner, oldVal, newVal)
        if not owner.init then
            return
        end
        
        owner.dirty = true
    end,
    
    ---@param owner DynamicCollectionViewModel
    ---@param oldVal function
    ---@param newVal function
    sort = function(owner, oldVal, newVal)
        if not owner.init then
            return
        end

        owner.dirty = true
    end,
    
    ---@param owner DynamicCollectionViewModel
    ---@param oldVal function
    ---@param newVal function
    group = function(owner, oldVal, newVal)
        if not owner.init then
            return
        end

        owner.dirty = true
    end,
    
    ---@param owner DynamicCollectionViewModel
    ---@param oldVal any
    ---@param newVal any
    dirty = function(owner, oldVal, newVal)
        if newVal then
            owner.dirty = false
        end
    end,
}

return DynamicCollectionViewModel