---
--- 文件名称:  SimpleCommandPool
--- 创建者:    yuancan
--- 创建时间:  2022/7/15 3:31 PM
-------------------------------------------------------------------
--- 功能描述：
---
---

local SimpleCommandWrap = require("Support/SimpleCommandWrap")

--模块
---@class SimpleCommandPool
local SimpleCommandPool = {}
---@type table<SimpleCommandWrap>
local usingItems = {}
---@type table<SimpleCommandWrap>
local freeItems = {}
local serialId = 1
local usingCount = 0

function SimpleCommandPool.Init()
  
end

---Acquire
---@param value SimpleCommand
function SimpleCommandPool.Acquire(value)
    ---@type SimpleCommandWrap
    local item = next(freeItems)
    if not item then
        local wrapItem = SimpleCommandWrap()
        usingItems[serialId] = wrapItem
        wrapItem.serialId = serialId
        serialId = serialId + 1
        usingCount = usingCount + 1
        wrapItem:SetAction(value.action)
        return wrapItem
    end
    
    item = freeItems[item]
    freeItems[item.serialId] = nil
    usingItems[item.serialId] = item
    item:SetAction(value.action)
    return item
end

---Recycle
---@param item SimpleCommandWrap
function SimpleCommandPool.Recycle(item)
    item:Clear()
    usingItems[item.serialId] = nil
    freeItems[item.serialId] = item
    usingCount = usingCount - 1
end

function SimpleCommandPool.Release()
    ---@type SimpleCommandWrap
    local item = next(freeItems)
    while item do
        local id = item
        item = freeItems[id]
        freeItems[id] = nil
        item:Release()
        item = next(freeItems)
    end
    item = next(usingItems)
    while item do
        local id = item
        item = usingItems[id]
        usingItems[id] = nil
        item:Release()
        item = next(freeItems)
    end
end


return SimpleCommandPool