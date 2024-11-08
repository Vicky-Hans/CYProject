---
--- 文件名称:  BaseServerDataViewModel
--- 创建者:    nieshihai
--- 创建时间:  2021/10/11 11:22
-------------------------------------------------------------------
--- 功能描述：
--- 也是数据源，只不过是服务器那边来的纯数据，有可能存在于整个游戏生命周期的，不会随着界面关闭Release掉。不想有除了数据以外的杂项，比如selected，command等
--- 所有使该数据内容改变的协议也可以放到这里面，做相关性的内聚
--- 会被组装到UI的VM里。Server数据变化时UI的VM需要得到对应的通知,相当于ServerData要被UI的View观察

local ObservableObject = require("Binding/ObservableObject")
local BindingManager = require("Binding/BindingManager")

--模块
---@class BaseServerDataViewModel
local BaseServerDataViewModel = bpcClass("BaseServerDataViewModel", ObservableObject)

--[[--
构造函数
]]
---@param t table
function BaseServerDataViewModel:Init(t)
    BaseServerDataViewModel.super.ctor(self, t)

    if self.onCreate ~= nil then
        self:onCreate(t)
    end

    BindingManager:AddSource(self,self.__classname)
end

function BaseServerDataViewModel:Release()
    BaseServerDataViewModel.super:InternalRelease()
    if self.onDestroy ~= nil then
        self:onDestroy()
    end
    
    BindingManager:RemoveSource(self)
end

return BaseServerDataViewModel