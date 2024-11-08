---
--- 文件名称:  BaseView
--- 创建者:    yuancan.
--- 创建时间:  2021/9/16 5:43 PM
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")
local UDH = require("UChatGlobalRequire")
local DataContext = require("DataContext/DataContext")
local LifeCycleBase = require("Common/LifeCycleBase")

---
--模块
---@class BaseView:LifeCycleBase
local BaseView = bpcClass("BaseView", LifeCycleBase)

function BaseView:Awake()
    ---@type DataContext
    self.dataContext = DataContext()
    self.dataContext.ownerView = self.__classname
    self.imageContainer = {}

    if self.viewModelName ~= nil then
        UDH.BindingManager:AddDataContext(self.dataContext,self.viewModelName)
    end

    if self.onAwake ~= nil then
        self:onAwake()
    end
end

function BaseView:Start()
    if self.onStart ~= nil then
        self:onStart()
    end

    self.dataContext:Build()
    self:RegisterLifeCycle()
end

--- 不支持嵌套属性访问
--- 请勿使用此方式直接更新VM或者ItemModel属性的值
---@param propName string
---@return any
function BaseView:GetSourceProp(propName)
    if not propName then
        return nil
    end

    if self.dataContext.source == nil then
        return nil
    end
    
    return self.dataContext.source[propName]
end

function BaseView:Enable()
    if self.onEnable ~= nil then
        self:onEnable()
    end
end

function BaseView:Disable()
    if self.onDisable ~= nil then
        self:onDisable()
    end
end


function BaseView:Destroy()
    --- 释放所有Sprite
    self.imageContainer = {}
    
    self:UnRegisterLifeCycle()
    
    if self.onDestroy ~= nil then
        self:onDestroy()
    end
    
    if self.dataContext ~= nil then
        if self.viewModelName ~= nil then
            -- 取消注册
            UDH.BindingManager:RemoveDataContext(self.dataContext)
        else
            --- 解除绑定
            self.dataContext:Unbind()
        end
        
        -- 释放对象
        self.dataContext = nil
    end
end

--- 实现对图片设置
---@param imageComp UnityEngine.UI.Image
---@param value string
function BaseView:SetSprite(imageComp,value)
    local oldValue = self.imageContainer[imageComp]
    --- 释放上一次设置的Sprite在已经设置的情况下
    if oldValue then
        IconNameConverter:Release(oldValue)
    end
    imageComp.sprite = IconNameConverter:ConvertFrom(value)
    self.imageContainer[imageComp] = value
end

return BaseView