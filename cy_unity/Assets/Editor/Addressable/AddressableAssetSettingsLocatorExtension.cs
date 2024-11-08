using System;
using System.Collections.Generic;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.ResourceManagement.ResourceLocations;

public static class AddressableAssetSettingsLocatorExtension
{
    private static AddressableAssetSettingsLocator locator;

    public static bool Locate(object key, Type type, out IList<IResourceLocation> locations)
    {
        if (locator == null)
        {
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
            locator = new AddressableAssetSettingsLocator(settings);
        }

        return locator.Locate(key, type, out locations);
    }

    public static string FindAddress(string guid)
    {
        var settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
        var entry = settings.FindAssetEntry(guid, false);
        return entry?.address ?? string.Empty;
    }
}