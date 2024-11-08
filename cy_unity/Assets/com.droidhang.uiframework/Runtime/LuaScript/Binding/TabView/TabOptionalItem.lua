---
--- 文件名称:  TabOptionalItem
--- 创建者:    nieshihai
--- 创建时间:  2021/10/20 14:35
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")

--模块
---@class TabOptionalItem
local TabOptionalItem = bpcClass("TabOptionalItem")
local LuaBindingTargetWrapper = CS.UIFramework.LuaBindingTargetWrapper
local PrefabManagerLua = CS.DH.UIFramework.PrefabManagerLua
local TextPathParser = require("Support/TextPathParser")
local ChainedSource = require("Binding/ChainedSource")

--[[--
构造函数
]]
---@param index number
---@param toggle UnityEngine.UI.Toggle @ui里的Toggle
---@param template string @该选项对应要加载的UI的addressable的路径
---@param propertyName string
function TabOptionalItem:ctor(index, toggle, parent, template, selectedCallback,propertyName)
    self.OnToggleValueChanged = function(sender, args)
        if sender ~= self.targetToggle then
            error("Invalid sender " .. tostring(self.targetToggle))
            return
        end
        
        local isOn = LuaBindingTargetWrapper.GetTargetValue(self.targetToggle)
        bpcPrintf("TabOptionalItem : "..self.index.."|"..tostring(isOn))
        if isOn and self.selectedCallback ~= nil then
            self.selectedCallback(self.index)
        end
    end
    
    self.targetToggle = LuaBindingTargetWrapper.GetTargetProxy(toggle, "isOn", "onValueChanged")
    self.targetToggle:ValueChanged("+", self.OnToggleValueChanged)
    
    self.selectedCallback = selectedCallback
    self.contentTemplate = template
    self.contentParent = parent
    self.index = index
    self.contentObj = nil
    self.isSelect = false
    ---@type DataContext
    self.dataContext = nil
    self.propertyName = propertyName
    
    self.onPropertyChanged = function()
        self:Bind(self.source)
    end

    ---@type TextPathParser
    if propertyName then
        local parser = TextPathParser(propertyName)
        ---@type Path
        local path = parser:Parse()
        ---@type PathToken
        self.token = path:AsPathToken()
    end

    if self.source ~= nil then
        self:Bind(self.source)
    end
end

function TabOptionalItem:Select(bRefreshToggleState)
    if self.contentObj ~= nil then
        self.contentObj:SetActive(true)
    elseif self.contentTemplate ~= nil then
        local templateType = type(self.contentTemplate)
        local isPrefab = templateType == 'userdata'
        
        if isPrefab then
            local instanceObj = CS.UnityEngine.GameObject.Instantiate(self.contentTemplate, self.contentParent)
            self:OnContentObjPrepared(instanceObj)
        else
            PrefabManagerLua.InstantiateWithPara(self.contentTemplate, {parent = self.contentParent}, function(go)
                self:OnContentObjPrepared(go)
            end)
        end
    end
    
    self.isSelect = true

    if bRefreshToggleState then
        LuaBindingTargetWrapper.SetTargetValue(self.targetToggle, self.isSelect, nil, nil)
    end
end

function TabOptionalItem:OnContentObjPrepared(go)
    self.contentObj = go
    PrefabManagerLua.ResetTransformInfo(go)
    local luaTable = LuaBindingTargetWrapper.GetLuaTable(go)
    self.dataContext = luaTable["dataContext"]
    if self.source ~= nil then
        self.dataContext:SetSource(self:GetSource())
    end
end

---@param bRefreshToggleState boolean @是否刷新状态
function TabOptionalItem:UnSelect(bRefreshToggleState)
    if self.contentObj ~= nil then
        self.contentObj:SetActive(false)
    end
    self.isSelect = false

    if bRefreshToggleState then
        LuaBindingTargetWrapper.SetTargetValue(self.targetToggle, self.isSelect, nil, nil)
    end
end

---@param source ObservableObject
function TabOptionalItem:Bind(source)
    if self.token == nil then
        self.source = source
        return
    end
    
    if self.source ~= nil then
        self.source:unsubscribe(self.propertyName,self.onPropertyChanged)
    end

    if source ~= nil and self.token ~= nil then
        self.source = ChainedSource(source, self.token)
    else
        self.source = source
    end
    
    if self.source ~= nil then
        self.source:subscribe(self.propertyName,self.onPropertyChanged)
    end

    if self.dataContext ~= nil then
        if self.source ~= nil then
            self.dataContext:SetSource(self:GetSource())
        else
            self.dataContext:SetSource(nil)
        end
    end
end

---@private
function TabOptionalItem:GetSource()
    if self.source then
        if self.source.GetValue then
            return self.source:GetValue()
        else
            return self.source
        end
    else
        return nil
    end
end

function TabOptionalItem:UnBind()
    if self.source ~= nil then
        self.source:unsubscribe(self.propertyName,self.onPropertyChanged)
    end
    
    if self.dataContext then
        self.dataContext:SetSource(nil)
    end
    
    if self.contentObj ~= nil then
        PrefabManagerLua.ReleaseInstance(self.contentObj)
    end

    self.contentObj = nil
    self.isSelect = false
    self.source = nil
end

return TabOptionalItem