---
--- 文件名称:  UIBaseViewModel
--- 创建者:    nieshihai
--- 创建时间:  2021/12/30 16:13
-------------------------------------------------------------------
--- 功能描述：
--- 
---

local BaseServerDataObservable = require("Binding/BaseServerDataObservable")
local SimpleCommand = require("Support/SimpleCommand")

---
--模块
---@class UIBaseViewModel:BaseServerDataObservable
local UIBaseViewModel = bpcClass("UIBaseViewModel", BaseServerDataObservable)

---@public
---@构造函数，有一个不定参数
function UIBaseViewModel:ctor(...)
    self.uiSerialId = 0 ---用于UIManager在关闭时反向查找uiView
    UIBaseViewModel.super.ctor(self, ...)
    
    self.closeWithVMCmd = SimpleCommand(function()
        UIManager:CloseWithVM(self)
    end)
    
    if self.onCreate ~= nil then
        self:onCreate(...)
    end
end

---@public
---@return string @UIConfig的Key， 默认是类名，如果覆盖就在子类里定义uiKey
function UIBaseViewModel:GetUIConfigKey()
    if self.uiKey ~= nil then
        return self.uiKey
    end
    
    return self.__classname
end

function UIBaseViewModel:Release()
    if self.disposed then
        return
    end

    self:InternalRelease()
end

function UIBaseViewModel:InternalRelease()
    if self.disposed then
        return
    end
    
    if self.onDestroy ~= nil then
        self:onDestroy()
    end

    UIBaseViewModel.super.InternalRelease(self)
end

return UIBaseViewModel