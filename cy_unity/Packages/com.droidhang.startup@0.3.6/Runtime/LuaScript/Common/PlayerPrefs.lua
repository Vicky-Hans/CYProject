local PlayerPrefs = { }

local CSPlayerPrefs = CS.UnityEngine.PlayerPrefs

---@param key string
---@param value number
function PlayerPrefs:SetInt(key, value)
    CSPlayerPrefs.SetInt(key, value)
    CSPlayerPrefs.Save()
end

---@param key string
---@return number
function PlayerPrefs:GetInt(key)
    return CSPlayerPrefs.GetInt(key)
end

---@param key string
---@param default number
---@return number
function PlayerPrefs:GetIntWithDefault(key, default)
    return CSPlayerPrefs.GetInt(key, default)
end

---@param key string
---@param value string
function PlayerPrefs:SetString(key, value)
    CSPlayerPrefs.SetString(key, value)
    CSPlayerPrefs.Save()
end

---@param key string
---@return string
function PlayerPrefs:GetString(key)
    return CSPlayerPrefs.GetString(key)
end

---@param key string
---@param default string
---@return string
function PlayerPrefs:GetStringWithDefault(key,default)
    return CSPlayerPrefs.GetString(key,default)
end

---@param key string
---@return boolean
function PlayerPrefs:HasKey(key)
    return CSPlayerPrefs.HasKey(key)
end

function PlayerPrefs:SetFloat(key, value)
    CSPlayerPrefs.SetFloat(key, value)
    CSPlayerPrefs.Save()
end

function PlayerPrefs:GetFloat(key)
    return CSPlayerPrefs.GetFloat(key)
end

function PlayerPrefs:GetFloatWithDefault(key, defalut)
    return CSPlayerPrefs.GetFloat(key, defalut)
end

function PlayerPrefs:DeleteKey(key)
    CSPlayerPrefs.DeleteKey(key)
end

function PlayerPrefs:DeleteAll()
    CSPlayerPrefs.DeleteAll()
end

return PlayerPrefs