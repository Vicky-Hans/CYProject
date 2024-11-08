---
--- 文件名称:  ObjectNameConverter
--- 创建者:    yuancan
--- 创建时间:  2022/2/11 2:44 PM
-------------------------------------------------------------------
--- 功能描述：
---
---

--模块
---@class ObjectNameConverter
local ObjectNameConverter = class("ObjectNameConverter")

--[[--
构造函数
]]
---@param t table
function ObjectNameConverter:ctor(t)

end

---@param assetPath string
---@return UnityEngine.Object
function ObjectNameConverter:ConvertFrom(assetPath)
    return CS.DH.Asset.AssetsManager.LoadAssetSync(assetPath)
end

---@param assetPath string
---@param complete fun(result:UnityEngine.Object)
function ObjectNameConverter:ConvertAsync(assetPath,complete)
    CS.DH.Asset.AssetsManager.LoadAssetAsync(assetPath,complete)
end

---@param object UnityEngine.Object
---@return string
function ObjectNameConverter:ConvertTo(object)
    return object.name
end

---@param assetPath string
function ObjectNameConverter:Release(assetPath)
    CS.DH.Asset.AssetsManager.Release(assetPath)
end

return ObjectNameConverter