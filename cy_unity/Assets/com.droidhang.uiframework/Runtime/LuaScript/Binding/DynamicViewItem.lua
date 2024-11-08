---
--- 文件名称:  DynamicViewItem
--- 创建者:    yuancan.
--- 创建时间:  2021/7/20 10:40 AM
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")

--模块
---@class DynamicViewItem
local DynamicViewItem = bpcClass("DynamicViewItem")

--[[--
构造函数
]]
---@param index number
---@param view GameObject
function DynamicViewItem:ctor(index, view, sortedIndex)
    self.index = index
    self.sortedIndex = sortedIndex
    self.view = view

    getmetatable(self).__tostring = function(table)
        return string.format("index:%s view:%s",table.index,table.view)
    end
end

return DynamicViewItem