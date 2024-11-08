---
--- 文件名称:  Binding
--- 创建者:    yuancan.
--- 创建时间:  2021/7/14 7:27 PM
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")
local UDH = require("UChatGlobalRequire")
local TextPathParser = require("Support/TextPathParser")
local ChainedSource = require("Binding/ChainedSource")
local CommandPool = require("Support/SimpleCommandPool")
local LuaBindingTargetWrapper = CS.UIFramework.LuaBindingTargetWrapper
local enableDebug = DataBindingDebug
--模块
---@class Binding
---@field cacheCommand SimpleCommandWrap
local Binding = bpcClass("Binding")

--[[--
构造函数
]]
function Binding:ctor()
    self.BindingMode = {
        OneWay = 0,
        TwoWay = 1,
        OneWayToSource = 2,
    }
    self.ControlFlags = {
        None = 0,
        ResetSourceValue = 1;
        ResetTargetValue = 2,
    }
    --- 缓存上一次设置的值，用于释放资源（当前适用于Sprite释放）
    self.previousValue = nil
    ---@type any
    self.source = nil
    ---@type string
    self.sourcePath = "" --如 'currentRole.name'
    ---@type any
    self.target = nil
    ---@type string
    self.targetPath = ""
    ---@type string
    self.sourcePropertyName = "" --如 'name'
    ---@type BindingMode
    self.mode = self.BindingMode.OneWay
    ---@type ControlFlags
    self.controlFlags = 0
    
    self.updateTargetTrigger = function()
        self:UpdateTarget()
    end
    
    self.OnSourcePropertyChanged = function(sender, propertyName, oldValue, newValue)
        assert(self:IsBound())
        if enableDebug then
            bpcPrintf("[Binding.OnSourcePropertyChanged]property name %s,old value %s new value %s", propertyName, oldValue, newValue)
        end
        if sender ~= self.source then
            error("Invalid sender " .. tostring(self.source))
            return
        end

        if propertyName ~= nil and propertyName ~= self.sourcePropertyName then
            return
        end

        self:UpdateTarget()
    end

    self.OnTargetPropertyChanged = function(sender, args)
        assert(self:IsBound())
        if enableDebug then
            bpcPrintf("[Binding.OnTargetPropertyChanged]old value %s new value %s", oldValue, newValue)
        end
        if sender ~= self.target then
            error("Invalid sender " .. tostring(self.source))
            return
        end

        self:UpdateSource()
    end
end

function Binding:IsBound()
    return self.source ~= nil
end

---@return Binding
function Binding:Target(target)
    self.target = target
    return self
end

---@param targetPath string
---@param updateTrigger UnityEngine.
---@return Binding
function Binding:For(targetPath, updateTrigger)
    assert(targetPath ~= nil)
    self.targetPath = targetPath
    self.updateTrigger = updateTrigger

    if type(self.target) ~= "table" then
        local target = LuaBindingTargetWrapper.GetTargetProxy(self.target, targetPath, updateTrigger)
        if target then
            self.target = target
        else
            self.targetIsTable = true
        end
    end

    return self
end

---@param sourcePath string
---@param converter any
---@return Binding
--- 如果converter传入function类型，则使用启用原数据根据converter修改的方式
function Binding:To(sourcePath, converter)
    assert(sourcePath ~= nil)
    self.sourcePath = sourcePath
    self.sourcePropertyName = UDH.BindingManager:GetPropertyPath(sourcePath)
    ---@type TextPathParser
    local parser = TextPathParser(sourcePath)
    ---@type Path
    local path = parser:Parse()
    ---@type PathToken
    self.token = path:AsPathToken()
    ---@type IconNameConverter
    local type = type(converter)
    if type == "function" then
        self.compute = converter
    else
        self.converter = converter
    end

    return self
end

---@param dataContext DataContext
function Binding:Build(dataContext)
    assert(dataContext ~= nil)
    dataContext:AddBinding(self)
    return self
end

function Binding:OneWay()
    self.mode = self.BindingMode.OneWay
    return self
end

function Binding:TwoWay()
    self.mode = self.BindingMode.TwoWay
    return self
end

function Binding:OneWayToSource()
    self.mode = self.BindingMode.OneWayToSource
    return self
end

-- @type #ObservableObject source
function Binding:Bind(source)
    if source == nil then
        error("source is null")
    end

    if self:IsBound() then
        self:Unbind()
    end

    --- chained source binding
    if self.token.path:Count() > 1 then
        self.source = ChainedSource(source, self.token)
    else
        if source == nil then
            error("binding source is null")
            return
        end

        self.source = source
    end

    if self.mode == self.BindingMode.OneWay or self.mode == self.BindingMode.TwoWay then
        self.source:subscribe(self.sourcePropertyName, self.OnSourcePropertyChanged)

        if self.converter and self.converter.UpdateTrigger then
            self.converter:UpdateTrigger(self.updateTargetTrigger)
        end
    end

    if self.mode == self.BindingMode.TwoWay or self.mode == self.BindingMode.OneWayToSource then
        if self.targetIsTable or type(self.target) == "table" then
            -- only used in collection view but collection view object will never changed
            --self.target:subscribe(self.targetPath,self.OnTargetPropertyChanged)
        else
            self.target:ValueChanged("+", self.OnTargetPropertyChanged)
        end
    end
    

    local sourceTarget = self.source[self.sourcePropertyName]
    self.sourceTargetIsEventHandler = sourceTarget ~= nil and type(sourceTarget) == "table" and sourceTarget.__classname == "EventHandler"

    self:InitValue()
end

function Binding:Unbind()
    if not self:IsBound() then
        return
    end

    if self.mode == self.BindingMode.OneWay or self.mode == self.BindingMode.TwoWay then
        self.source:unsubscribe(self.sourcePropertyName, self.OnSourcePropertyChanged)
    end

    if self.mode == self.BindingMode.TwoWay or self.mode == self.BindingMode.OneWayToSource then
        if self.targetIsTable or type(self.target) == "table" then
            -- only used in collection view but collection view object will never changed
            --self.target:unsubscribe(self.targetPath,self.OnTargetPropertyChanged)
        else
            self.target:ValueChanged("-", self.OnTargetPropertyChanged)
        end
    end

    self:ReleaseTargetValue()

    self:ResetValue()

    if self.source["Unbind"] then
        self.source:Unbind()
    end
    self:ReleaseCommand()
    self.source = nil
end

function Binding:UpdateTarget()
    if not self:IsBound() then
        return
    end

    local value
    local actualSource = self.source
    if self.source.__classname == "ChainedSource" then
        value = self.source:GetValue()
        actualSource = self.source:GetSource()
    else
        value = self.source[self.sourcePropertyName]
    end

    if value ~= nil and type(value) == "function" then
        value = value(actualSource) --- 传'self'进去
    end

    local targetType = type(self.target)
    if enableDebug then
        if targetType == "table" then
            bpcPrintf("[Binding:UpdateTarget] 设置lua表 %s 的 %s 为 %s", self.target.__classname, self.targetPath, value)
        else
            bpcPrintf("[Binding:UpdateTarget] 设置C#Proxy对象 %s 的 %s 为 %s",
                    LuaBindingTargetWrapper.GetTargetName(self.target), self.targetPath, value)
        end
    end
    if value and type(value) == "table" and value.__classname == 'SimpleCommand' then
        if  not self.cacheCommand then
            self.cacheCommand = CommandPool.Acquire(value)
        else
            self.cacheCommand:SetAction(value.action)
        end
    end

    local convertedValue = value
    --- 在使用了计算属性或者类型转换器的情况下，强制converter不为空
    --- c#在converter不为空的情况下，使用转换后的新值
    local translator = nil
    --- 使用匿名委托修改原始值
    if self.compute ~= nil then
        convertedValue = self.compute(value)
        translator = {}
    --- 使用类型转换器转换原始值，适用于图片路径转换为图片    
    elseif self.converter ~= nil then
        if self.previousValue and self.previousValue ~= value then
            --- 释放上一次设置的对象
            self.converter:Release(self.previousValue)
        end
        
        self.previousValue = value
        convertedValue = self.converter:ConvertFrom(value)
        translator = self.converter
    end

    --- 使用可复用的SimpleCommand
    if self.cacheCommand then
        value = self.cacheCommand.csCommand
    end

    if self.targetIsTable or targetType == "table" then
        if type(self.target[self.targetPath]) == "function" then
            self.target[self.targetPath](self.target,value)
        else
            self.target[self.targetPath] = convertedValue
        end
    else
        LuaBindingTargetWrapper.SetTargetValue(self.target, value, translator ~= nil, convertedValue)
    end
end

function Binding:UpdateSource()
    if not self:IsBound() then
        return
    end

    local tType = type(self.target)
    local value = nil

    if self.targetIsTable or tType == "table" then
        if not self.sourceTargetIsEventHandler and type(self.target[self.targetPath]) == "function" then
            value = self.target[self.targetPath](self.target)
        else
            value = self.target[self.targetPath]
        end
    else
        value = LuaBindingTargetWrapper.GetTargetValue(self.target)
    end

    if enableDebug then
        if tType == "table" then
            bpcPrintf("[Binding:UpdateSource] 设置Source为lua表 %s 的 %s（%s）", self.target.__classname, self.targetPath, value)
        else
            bpcPrintf("[Binding:UpdateSource] 设置Source为C#Proxy对象 %s 的 %s（%s）",
                    LuaBindingTargetWrapper.GetTargetName(self.target), self.targetPath, value)
        end
    end

    if self.source.__classname == "ChainedSource" then
        self.source:SetValue(value)
    else
        if self.sourceTargetIsEventHandler then
            local sourceTarget = self.source[self.sourcePropertyName]
            sourceTarget:AddListener(value)
        else
            self.source[self.sourcePropertyName] = value
        end
    end
end

function Binding:InitValue()
    if self.mode == self.BindingMode.OneWay then
        self:UpdateTarget()
    elseif self.mode == self.BindingMode.TwoWay then
        self:UpdateTarget()
    elseif self.mode == self.BindingMode.OneWayToSource then
        self:UpdateSource()
    else
        error("Invalid mode " .. tostring(self.mode))
    end
end

function Binding:ResetValue()
    if self.mode == self.BindingMode.OneWay then

    elseif self.mode == self.BindingMode.TwoWay then

    elseif self.mode == self.BindingMode.OneWayToSource then
        if self.source.__classname == "ChainedSource" then
            if self.controlFlags & self.ControlFlags.ResetSourceValue then
                self.source:SetValue(nil)
            end
        else
            local sourceTarget = self.source[self.sourcePropertyName]
            if sourceTarget == nil then
                return
            end
            if type(sourceTarget) == "table" and sourceTarget.__classname == "EventHandler" then
                sourceTarget:RemoveListener(self.target[self.targetPath])
            else
                if self.controlFlags & self.ControlFlags.ResetSourceValue then
                    self.source[self.sourcePropertyName] = nil
                end
            end
        end
    else
        error("Invalid mode " .. tostring(self.mode))
    end
end

function Binding:ReleaseCommand()
    if self.cacheCommand then
        CommandPool.Recycle(self.cacheCommand)
    end
    
    self.cacheCommand = nil
end

--- release target value to null
--- when converter enabled
function Binding:ReleaseTargetValue()
    --- ignore nil converter
    if self.converter == nil then
        return
    end

    if not self:IsBound() then
        return
    end

    local type = type(self.target)
    if type == "table" then
        --self.target[self.targetPath] = nil
        -- if needed?
    else
        local value
        if self.source.__classname == "ChainedSource" then
            value = self.source:GetValue()
        else
            value = self.source[self.sourcePropertyName]
        end
        --- release target value reference
        LuaBindingTargetWrapper.SetTargetValue(self.target, value, self.converter ~= nil, nil)
        --- release reference of resource
        if self.previousValue then
            self.converter:Release(self.previousValue)
        end
    end
end

return Binding