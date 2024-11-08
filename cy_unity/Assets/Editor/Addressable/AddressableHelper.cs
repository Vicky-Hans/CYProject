using UnityEditor.AddressableAssets.Settings;

public static class AddressableHelper
{
    public static void RemoveMissingGroupReferences(AddressableAssetSettings settings)
    {
        settings.RemoveMissingGroupReferences();
    }
}