---
--- 文件名称:  IconNameConverter
--- 创建者:    yuancan.
--- 创建时间:  2021/7/22 10:55 AM
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")
local LuaBindingTargetWrapper = CS.UIFramework.LuaBindingTargetWrapper
--模块
---@class IconNameConverter
local IconNameConverter = bpcClass("IconNameConverter")

--[[--
构造函数
]]
---@param t table
function IconNameConverter:ctor(t)
    
end

---@param iconName string
---@return UnityEngine.Sprite
function IconNameConverter:ConvertFrom(iconName)
    return LuaBindingTargetWrapper.LoadSprite(iconName)
end

---@param sprite UnityEngine.Sprite
---@return string
function IconNameConverter:ConvertTo(sprite)
    return sprite.name
end

---@param iconName string
function IconNameConverter:Release(iconName)
    LuaBindingTargetWrapper.ReleaseSprite(iconName)
end

return IconNameConverter