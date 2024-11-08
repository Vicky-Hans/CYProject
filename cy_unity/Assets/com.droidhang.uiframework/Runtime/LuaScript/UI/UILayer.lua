---
--- 文件名称:  UILayer
--- 创建者:    nieshihai
--- 创建时间:  2021/10/26 16:26
-------------------------------------------------------------------
--- 功能描述：
--- UI分组
---

--模块
---@class UILayer
local UILayer = bpcClass("UILayer")

local List = require("Common/List")
local MenuManagerWrapper = CS.DH.UIFramework.MenuManagerWrapper
local DEFAULT_ORDER = 1000

--[[--
构造函数
]]
function UILayer:ctor(layerRoot, layerIndex)
    self.layerRoot = layerRoot
    self.layerIndex = layerIndex
    self.uiList = List:New(string) -- 存uikey

    self.layerRoot:Stretch()
end

---@public 把UI加到当前UILayer的列表里
function UILayer:AddUI(uiKey)
    if not self.uiList:Contains(uiKey) then
        self.uiList:Add(uiKey)
    end
end

---@public 把UI从当前UILayer的列表移除
function UILayer:RemoveUI(uiKey)
    if self.uiList:Contains(uiKey) then
        self.uiList:Remove(uiKey)
    end
end

---@public 把列表里的UI重新排序，uiKey对应的UI提到最上面
function UILayer:Take2Top(uiKey)
    self.uiList:Remove(uiKey)
    self.uiList:Add(uiKey)

    local index = 0

    self.uiList:ForEach(function(uiKey)
        index = index + 1
        MenuManagerWrapper.ResortMenuOrder(uiKey, DEFAULT_ORDER * (self.layerIndex - 1) + index * 5)
    end)
end

function UILayer:GetNextOrder()
    return DEFAULT_ORDER * (self.layerIndex - 1) + (self.uiList:Count() + 1) * 5
end

function UILayer:GetCurMaxOrder()
    return DEFAULT_ORDER * (self.layerIndex - 1) + self.uiList:Count() * 5
end

return UILayer