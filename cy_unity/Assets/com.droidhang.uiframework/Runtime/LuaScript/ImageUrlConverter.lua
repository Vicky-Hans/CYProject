---
--- 文件名称:  ImageUrlConverter
--- 创建者:    nieshihai
--- 创建时间:  2022/3/6 10:55 AM
-------------------------------------------------------------------
--- 功能描述：
--- 将url转化成image
---

--模块
---@class DownLoadState
---@field None number
---@field Loading number
---@field Loaded number
---@field Error number
local DownLoadState = {
    None = 1,
    Loading = 2,
    Loaded = 3,
    Error = 4,
}

require("Common/System")
local DownLoadImageFromUrl = CS.DH.UIFramework.DownLoadImageFromUrl
local Vector2 = CS.UnityEngine.Vector2


--模块
---@class ImageUrlConverter
local ImageUrlConverter = bpcClass("ImageUrlConverter")

function ImageUrlConverter:ctor(loadingSprite, errorSprite, callback ,startDownBack)
    self.loadingSprite = loadingSprite
    self.errorSprite = errorSprite
    self.loadState = DownLoadState.None
    self.lastUrl = nil
    self.callback = callback
    self.startDownBack = startDownBack
    self.imageRect = nil

    self.onLoadedCallback = function(loadedSprite)
        self:OnLoadedCallback(loadedSprite)
    end

    self.onErrorCallback = function()
        self:OnErrorCallback()
    end
end

function ImageUrlConverter:UpdateTrigger(triggerCallback)
    self.triggerCallback = triggerCallback
end

---@param url string
---@return UnityEngine.Sprite
function ImageUrlConverter:ConvertFrom(url)
    if self.startDownBack then
        self.startDownBack()
    end
    if self.lastUrl and self.lastUrl ~= url then
        self:Release()
        self.lastUrl = nil
    end

    if url == nil then
        return self.loadingSprite
    end

    --- 第一次加载url
    if self.lastUrl == nil then
        self.lastUrl = url
        self.loadState = DownLoadState.Loading
        self.downloadComponent = DownLoadImageFromUrl.Get()
        self.downloadComponent:Load(url):WithLoadedAction(self.onLoadedCallback):WithErrorAction(self.onErrorCallback):StartLoad()
    end

    if self.loadState == DownLoadState.Loading then
        return self.loadingSprite
    end

    if self.loadState == DownLoadState.Error then
        return self.errorSprite
    end

    if self.loadState == DownLoadState.Loaded then
        return self.loadedTexture
    end

    return nil
end

function ImageUrlConverter:OnLoadedCallback(loadedTexture)
    self.loadState = DownLoadState.Loaded
    self.loadedTexture = loadedTexture

    if self.triggerCallback then
        self.triggerCallback()
    end

    if self.callback then
        self.callback(self.loadedTexture)
    end
end

function ImageUrlConverter:OnErrorCallback()
    self.loadState = DownLoadState.Error
    self.loadedTexture = nil
end

---@param url string
function ImageUrlConverter:Release()
    if self.lastUrl ~= nil then
        if not bpcIsNull(self.downloadComponent) then
            self.downloadComponent:WithLoadedAction(nil):WithErrorAction(nil)
            self.downloadComponent = nil
        end
        
        DownLoadImageFromUrl.ClearCache(self.lastUrl)

        self.loadedTexture = nil
        self.lastUrl = nil
    end
end

function ImageUrlConverter:ClearAllData()
    DownLoadImageFromUrl.ClearAllCachedFiles()
end

return ImageUrlConverter