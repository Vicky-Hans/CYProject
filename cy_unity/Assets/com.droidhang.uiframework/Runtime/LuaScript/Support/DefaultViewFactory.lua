---
--- 文件名称:  DefaultViewFactory
--- 创建者:    yuancan.
--- 创建时间:  2021/7/16 4:54 PM
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")
local LuaBindingTargetWrapper = CS.UIFramework.LuaBindingTargetWrapper
local PrefabManagerLua = CS.DH.UIFramework.PrefabManagerLua
--模块
---@class DefaultViewFactory
local DefaultViewFactory = bpcClass("DefaultViewFactory")

--[[--
构造函数
@param #table self
@param #table t 初始化参数
]]
function DefaultViewFactory:ctor(t, pool)
   self.pool = pool
   ---@type GameObject
   self.viewTemplate = nil
   self.preparePrefab = false
   ---@type function callback function for owner
   self.onPrefabPrepared = nil
end

function DefaultViewFactory:PrepareTemplate()
   --- 若传入的是Prefab引用，不再需要加载Prefab
   if type(self.viewTemplate) == "userdata" then
      self.preparePrefab = true
      --- callback for update ui
      if self.onPrefabPrepared then
         self.onPrefabPrepared()
      end
      return
   end
   
   PrefabManagerLua.PreparePrefab(self.viewTemplate, function(prefab)
      self.viewTemplate = prefab
      self.preparePrefab = true
      --- callback for update ui
      if self.onPrefabPrepared then
         self.onPrefabPrepared()
      end
   end)
end

function DefaultViewFactory:CreateViewItem()
   local newItem
   if self.pool ~= nil then
      newItem = self.pool:GetViewItem()
   else
      newItem = CS.UnityEngine.GameObject.Instantiate(self.viewTemplate)
      if not newItem.activeSelf then
         newItem:SetActive(true)
      end
   end

   LuaBindingTargetWrapper.SetParent(newItem,self.content,false)
   return newItem
end

---@param GameObject
---@public
function DefaultViewFactory:ReleaseViewItem(viewItem)
   if viewItem == nil or bpcIsNull(viewItem) then
      return
   end
   
   if self.pool ~= nil then
      self.pool:ReleaseViewItem(viewItem)
   else
      -- just destroy viewItem
      CS.UnityEngine.GameObject.Destroy(viewItem)
   end
end

function DefaultViewFactory:Release()
   self.pool = nil
   self.viewTemplate = nil
end

return DefaultViewFactory