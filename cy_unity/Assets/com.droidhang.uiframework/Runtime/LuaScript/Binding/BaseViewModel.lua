---
--- 文件名称:  BaseViewModel
--- 创建者:    yuancan.
--- 创建时间:  2021/9/17 9:50 AM
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")
local UDH = require("UChatGlobalRequire")
local BaseServerDataObservable = require("Binding/BaseServerDataObservable")

---
--模块
---@class BaseViewModel:BaseServerDataObservable
local BaseViewModel = bpcClass("BaseViewModel", BaseServerDataObservable)

--[[--
构造函数
]]
---@param t table
function BaseViewModel:ctor(...)
    BaseViewModel.super.ctor(self, ...)

    if self.onCreate ~= nil then
        self:onCreate(...)
    end

    UDH.BindingManager:AddSource(self,self.__classname)
end

function BaseViewModel:Release()
    if self.disposed then
        return
    end
    
    self:InternalRelease()
    UDH.BindingManager:RemoveSource(self)
end

function BaseViewModel:InternalRelease()
    if self.disposed then
        return
    end

    BaseViewModel.super.InternalRelease(self)
    if self.onDestroy ~= nil then
        self:onDestroy()
    end
end

return BaseViewModel