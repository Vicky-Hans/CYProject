---
--- 文件名称:  CircularDynamicGroupItem
--- 创建者:    nieshihai
--- 创建时间:  2021/9/24 16:03
-------------------------------------------------------------------
--- 功能描述：
--- 用于分组扩展的循环列表，记录一个DynamicView的GroupItem，以及该group里对DynamicView的indexedList列表的索引列表
---

require("Common/System")
local List = require("Common/List")

---
--模块
---@class CircularDynamicGroupItem
---@field groupSource any @groupItem对应ViewModel
---@field groupIndex number @用来在界面上显示的排序
---@field indexedList number[] @该组里显示的item索引的列表
local CircularDynamicGroupItem = bpcClass("CircularDynamicGroupItem")

--[[--
构造函数
]]
---@param t table
function CircularDynamicGroupItem:ctor(groupSource, groupIndex)
    self.groupSource = groupSource
    self.groupIndex = groupIndex
    self.indexedList = List:New() --是DynamicView的indexedList列表的索引列表
end

return CircularDynamicGroupItem