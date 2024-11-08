---
--- 文件名称:  LocalSaveData
--- 创建者:    nieshihai
--- 创建时间:  2022/5/7 17:59
-------------------------------------------------------------------
--- 功能描述：
---
---
--模块
---@class LocalSaveData
local LocalSaveData = bpcClass("LocalSaveData")

function LocalSaveData:SetString(key, value)
    PlayerPrefs:SetString(key, value)
end

function LocalSaveData:GetString(key, defaultValue)
    if defaultValue then
        return PlayerPrefs:GetStringWithDefault(key, defaultValue)
    else
        return PlayerPrefs:GetString(key)
    end
end

function LocalSaveData:SetInt(key, value)
    PlayerPrefs:SetInt(key, value)
end

function LocalSaveData:GetInt(key, defaultValue)
    if defaultValue then
        return PlayerPrefs:GetIntWithDefault(key, defaultValue)
    else
        return PlayerPrefs:GetInt(key)
    end
end

function LocalSaveData:SetFloat(key, value)
    PlayerPrefs:SetFloat(key, value)
end

function LocalSaveData:GetFloat(key, defaultValue)
    if defaultValue then
        return PlayerPrefs:GetFloatWithDefault(key, defaultValue)
    else
        return PlayerPrefs:GetFloat(key)
    end
end

function LocalSaveData:HasKey(key)
    return PlayerPrefs:HasKey(key)
end

function LocalSaveData:DeleteKey(key)
    PlayerPrefs:DeleteKey(key)
end

--- LocalData
local LOCAL_DATA_KEY = {
    LastLoginServerKey = "lastLoginServer",
    Account = "ACCOUNT",
}

function LocalSaveData:SaveRecordServerId(id)
    self:SetInt(LOCAL_DATA_KEY.LastLoginServerKey, id)
end

function LocalSaveData:GetRecordServerId()
    return self:GetInt(LOCAL_DATA_KEY.LastLoginServerKey, -1)
end

function LocalSaveData:SaveRecordAccount(id)
    self:SetString(LOCAL_DATA_KEY.Account, id)
end

function LocalSaveData:GetRecordAccount()
    return self:GetString(LOCAL_DATA_KEY.ACCOUNT)
end

function LocalSaveData:DelRecordAccount()
    self:DeleteKey(LOCAL_DATA_KEY.ACCOUNT)
end

return LocalSaveData