---
--- 文件名称:  UITree
--- 创建者:    nieshihai
--- 创建时间:  2021/10/26 15:15
-------------------------------------------------------------------
--- 功能描述：
--- 初始化和管理UILayer
---

--模块
---@class UITree
local UITree = bpcClass("UITree")
local UnityType = require("Common/UnityType")
local Dictionary = require("Common/Dictionary")
local List = require("Common/List")
local UILayersConfig = require("UI/UILayersConfig")
local UILayer = require("UI/UILayer")

local PrefabManagerLua = CS.DH.UIFramework.PrefabManagerLua
local GameObject = CS.UnityEngine.GameObject
local LuaBindingTargetWrapper = CS.UIFramework.LuaBindingTargetWrapper

function UITree:Init(finishCallback)
    self.layersDic = Dictionary:New(number, table)
    local uiRoot = CS.UnityEngine.GameObject.Find("UITree")
    if uiRoot then
        self:PrepareUITree(uiRoot, finishCallback)
        return
    end
    
    PrefabManagerLua.Instantiate("UITree", function(uiTree)
        self:PrepareUITree(uiTree, finishCallback)
    end)
end

---@private
function UITree:PrepareUITree(treeObj, finishCallback)
    GameObject.DontDestroyOnLoad(treeObj)
    local treeTrans = treeObj.transform
    
    self.uiCamera = treeTrans:GetAutoInjectNode(".", ".", "UnityEngine.Camera")
    local uiRoot = treeTrans:GetAutoInjectNode("Canvas", "Canvas", UnityType.Transform)
    self:InitUILayers(uiRoot)
    finishCallback()
end

---@private
---@param uiRoot Transform
function UITree:InitUILayers(uiRoot)
    local tmpDic = Dictionary:New(number, string)
    local tmpKeyList = List:New(number)

    for layerName, i in pairs(UILayersConfig) do
        if tmpDic:ContainsKey(i) then
            error("UILayersConfig 里有重复的id："..i)
        end

        tmpKeyList:Add(i)
        tmpDic:Add(i, layerName)
    end

    tmpKeyList:Sort()

    tmpKeyList:ForEach(function(id)
        local layerName = tmpDic[id]

        local layerRoot = GameObject(layerName)
        layerRoot:AddComponent(typeof(CS.UnityEngine.RectTransform))
        LuaBindingTargetWrapper.SetParent(layerRoot, uiRoot)

        local layer = UILayer(layerRoot.transform, id)
        self.layersDic:Add(id, layer)
    end)
end

function UITree:GetLayer(layer)
    if self.layersDic:ContainsKey(layer) then
        return self.layersDic[layer]
    end

    return nil
end

return UITree