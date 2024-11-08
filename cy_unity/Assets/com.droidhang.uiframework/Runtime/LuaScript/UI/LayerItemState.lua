---
--- 文件名称:  LayerItemState
--- 创建者:    nieshihai
--- 创建时间:  2022/2/15 10:36
-------------------------------------------------------------------
--- 功能描述：
---
---

--模块
---@class LayerItemState
---@field None number
---@field Loading number
---@field Open number
---@field OnCover number
---@field OnRefocus number
---@field Release number
local LayerItemState = {
    None = 1,
    Loading = 2,
    Open = 3,
    OnCover = 4,
    OnRefocus = 5,
    Release = 6
}

return LayerItemState