/// This AnalyzeRule is based on the built-in rule CheckBundleDupeDependencies
/// This rule finds assets in Addressables that will be duplicated across multiple AssetBundles
/// Instead of placing all problematic assets in a shared Group, this rule results in fewer AssetBundles
/// being created by placing assets with the same AssetBundle parents into the same label and AssetBundle

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Pipeline;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets.Build.AnalyzeRules;

public class CheckBundleDupeDependenciesV2 : BundleRuleBase
{
    private struct DuplicateResult
    {
        public AddressableAssetGroup Group;
        public string DuplicatedFile;
        public string AssetPath;
        public GUID DuplicatedGroupGuid;
    }

    // Return true because we have added an automated way of fixing these problems with the FixIssues() function
    public override bool CanFix => true;

    // The name that appears in the Editor UI
    public override string ruleName => "Check Duplicate Bundle Dependencies V2";

    public static readonly string GroupName = "DuplicateAssetsSortedByLabel";
    public event Func<string, bool> CheckUISprite;

    [NonSerialized] internal readonly Dictionary<string, Dictionary<string, List<string>>> m_AllIssues = new();
    [SerializeField] internal Dictionary<List<string>, List<string>> duplicateAssetsAndParents = new();

    // The function that is called when the user clicks "Analyze Selected Rules" in the Analyze window
    public override List<AnalyzeResult> RefreshAnalysis(AddressableAssetSettings settings)
    {
        ClearAnalysis();
        return CheckForDuplicateDependencies(settings);
    }

    private List<AnalyzeResult> CheckForDuplicateDependencies(AddressableAssetSettings settings)
    {
        // Create a container to store all our AnalyzeResults
        var retVal = new List<AnalyzeResult>();

        // Quit if the opened scene is not saved
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.LogError("Cannot run Analyze with unsaved scenes");
            retVal.Add(new AnalyzeResult { resultName = ruleName + "Cannot run Analyze with unsaved scenes" });
            return retVal;
        }

        // Internal Addressables function that populates m_AllBundleInputDefs with what all our bundles will look like
        CalculateInputDefinitions(settings);

        // Early return if we found no bundles to build
        if (m_AllBundleInputDefs.Count <= 0) return retVal;

        var context = GetBuildContext(settings);
        var exitCode = RefreshBuild(context);
        if (exitCode < ReturnCode.Success)
        {
            Debug.LogError("Analyze build failed. " + exitCode);
            retVal.Add(new AnalyzeResult { resultName = ruleName + "Analyze build failed. " + exitCode });
            return retVal;
        }

        var implicitGuids = GetImplicitGuidToFilesMap();

        // Actually calculate the duplicates
        var dupeResults = CalculateDuplicates(implicitGuids, context);
        BuildImplicitDuplicatedAssetsSet(dupeResults);

        retVal = (from issueGroup in m_AllIssues
            from bundle in issueGroup.Value
            from item in bundle.Value
            select new AnalyzeResult
            {
                resultName = ruleName + kDelimiter +
                             issueGroup.Key + kDelimiter +
                             ConvertBundleName(bundle.Key, issueGroup.Key) + kDelimiter +
                             item,
                severity = MessageType.Warning
            }).ToList();

        if (retVal.Count == 0)
            retVal.Add(noErrors);

        return retVal;
    }

    private IEnumerable<DuplicateResult> CalculateDuplicates(Dictionary<GUID, List<string>> implicitGuids,
        AddressableAssetsBuildContext aaContext)
    {
        duplicateAssetsAndParents.Clear();

        //Get all guids that have more than one bundle referencing them
        var validGuids =
            from dupeGuid in implicitGuids
            where dupeGuid.Value.Distinct().Count() > 1
            where IsValidPath(AssetDatabase.GUIDToAssetPath(dupeGuid.Key.ToString()))
            select dupeGuid;

        // Key = a set of bundle parents
        // Value = asset paths that share the same bundle parents
        // e.g. <{"bundle1", "bundle2"} , {"Assets/Sword_D.tif", "Assets/Sword_N.tif"}>

        foreach (var entry in validGuids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(entry.Key.ToString());
            // Grab the list of bundle parents
            var assetParents = entry.Value;

            // Purge duplicate parents (assets inside a Scene can show multiple copies of the Scene AssetBundle as a parent)
            var nonDupeParents = new List<string>();
            foreach (var parent in assetParents)
            {
                if (nonDupeParents.Contains(parent))
                    continue;
                nonDupeParents.Add(parent);
            }

            assetParents = nonDupeParents;

            // Add this pair to the dictionary
            var found = false;
            foreach (var bundleParentSetup in duplicateAssetsAndParents.Keys)
                // If this set of bundle parents equals our set of bundle parents, add this asset to this dictionary entry
                if (Enumerable.SequenceEqual(bundleParentSetup, assetParents))
                {
                    duplicateAssetsAndParents[bundleParentSetup].Add(assetPath);
                    found = true;
                    break;
                }

            if (!found)
                // We failed to find an existing set of matching bundle parents. Add a new entry
                duplicateAssetsAndParents.Add(assetParents, new List<string>() { assetPath });
        }

        return
            from guidToFile in validGuids
            from file in guidToFile.Value

            //Get the files that belong to those guids
            let fileToBundle = m_ExtractData.WriteData.FileToBundle[file]

            //Get the bundles that belong to those files
            let bundleToGroup = aaContext.bundleToAssetGroup[fileToBundle]

            //Get the asset groups that belong to those bundles
            let selectedGroup =
                aaContext.Settings.FindGroup(findGroup => findGroup != null && findGroup.Guid == bundleToGroup)
            select new DuplicateResult
            {
                Group = selectedGroup,
                DuplicatedFile = file,
                AssetPath = AssetDatabase.GUIDToAssetPath(guidToFile.Key.ToString()),
                DuplicatedGroupGuid = guidToFile.Key
            };
    }

    private void BuildImplicitDuplicatedAssetsSet(IEnumerable<DuplicateResult> dupeResults)
    {
        foreach (var dupeResult in dupeResults)
        {
            // Add the data to the AllIssues container which is shown in the Analyze window
            Dictionary<string, List<string>> groupData;
            if (!m_AllIssues.TryGetValue(dupeResult.Group.Name, out groupData))
            {
                groupData = new Dictionary<string, List<string>>();
                m_AllIssues.Add(dupeResult.Group.Name, groupData);
            }

            // TODO: why is this necessary?
            List<string> assets;
            if (!groupData.TryGetValue(m_ExtractData.WriteData.FileToBundle[dupeResult.DuplicatedFile], out assets))
            {
                assets = new List<string>();
                groupData.Add(m_ExtractData.WriteData.FileToBundle[dupeResult.DuplicatedFile], assets);
            }

            assets.Add(dupeResult.AssetPath);
        }
    }


    public void ClearGroupData(AddressableAssetSettings settings)
    {
        var desiredGroupName = GroupName;
        var group = settings.FindGroup(desiredGroupName);
        if (group == null)
        {
            group = settings.CreateGroup(desiredGroupName, false, false, false, null, typeof(BundledAssetGroupSchema),
                typeof(ContentUpdateGroupSchema));
            var bundleSchema = group.GetSchema<BundledAssetGroupSchema>();
            // Set to pack by label so that assets with the same label are put in the same AssetBundle
            bundleSchema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogetherByLabel;
        }

        var entries = new List<AddressableAssetEntry>();
        entries.AddRange(group.entries);
        group.RemoveAssetEntries(entries, false);
    }

    // The function that is called when the user clicks "Fix Issues" in the Analyze window
    public override void FixIssues(AddressableAssetSettings settings)
    {
        // If we have no duplicate data, run the check again
        if (duplicateAssetsAndParents == null || duplicateAssetsAndParents.Count == 0)
            CheckForDuplicateDependencies(settings);

        // If we have found no duplicates, return
        if (duplicateAssetsAndParents.Count == 0)
            return;

        // Setup a new Addressables Group to store all our duplicate assets
        var desiredGroupName = GroupName;
        var group = settings.FindGroup(desiredGroupName);
        if (group == null)
        {
            group = settings.CreateGroup(desiredGroupName, false, false, false, null, typeof(BundledAssetGroupSchema),
                typeof(ContentUpdateGroupSchema));
            var bundleSchema = group.GetSchema<BundledAssetGroupSchema>();
            // Set to pack by label so that assets with the same label are put in the same AssetBundle
            bundleSchema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogetherByLabel;
        }

        try
        {
            EditorUtility.DisplayProgressBar("Setting up De-Duplication Group...", "",
                0f / duplicateAssetsAndParents.Count);
            // Iterate through each duplicate asset
            var bundleNumber = 1;
            foreach (var entry in duplicateAssetsAndParents)
            {
                EditorUtility.DisplayProgressBar("Setting up De-Duplication Group...", "Creating Label Group",
                    (float)bundleNumber / duplicateAssetsAndParents.Count);

                var entriesToAdd = new List<AddressableAssetEntry>();
                var hashCode = 0;
                // Put each asset in the shared Group
                foreach (var assetPath in entry.Value)
                {
                    if (CheckUISprite?.Invoke(assetPath) ?? false)
                        throw new Exception($"[dherror] UI图片必须打图集、不能被其他引用导致冗余、不能使用RawImage方式引用:{assetPath}");

                    var entryItem = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(assetPath).ToString(),
                        group,
                        false, false);
                    if (assetPath.EndsWith(".shader"))
                    {
                        // 所有shader打入一个Label
                        var desiredLabelName = "BundleShader";
                        // Set the label for this selection of assets so they get packed into the same AssetBundle
                        settings.AddLabel(desiredLabelName);
                        entryItem.SetLabel(desiredLabelName, true);
                        continue;
                    }

                    hashCode ^= assetPath.GetHashCode();
                    entriesToAdd.Add(entryItem);
                }

                if (entriesToAdd.Count > 0)
                {
                    // Create a new Label
                    var desiredLabelName = "Bundle" + (uint)hashCode;
                    // Set the label for this selection of assets so they get packed into the same AssetBundle
                    settings.AddLabel(desiredLabelName);
                    SetLabelValueForEntries(settings, entriesToAdd, desiredLabelName, true);
                }

                bundleNumber++;
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    // Helper function for adding labels to Addressable assets
    private void SetLabelValueForEntries(AddressableAssetSettings settings, List<AddressableAssetEntry> entries,
        string label, bool value, bool postEvent = true)
    {
        if (value)
            settings.AddLabel(label);

        foreach (var e in entries)
            e.SetLabel(label, value, false);

        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, entries, postEvent, true);
    }

    // The function that is run when the user clicks "Clear Selected Rules" in the Analyze window
    public override void ClearAnalysis()
    {
        m_AllIssues.Clear();
        duplicateAssetsAndParents.Clear();
        base.ClearAnalysis();
    }
}

// Boilerplate to add our rule to the AnalyzeSystem's list of rules
[InitializeOnLoad]
internal class RegisterCheckBundleDupeDependenciesV2
{
    static RegisterCheckBundleDupeDependenciesV2()
    {
        AnalyzeSystem.RegisterNewRule<CheckBundleDupeDependenciesV2>();
    }
}