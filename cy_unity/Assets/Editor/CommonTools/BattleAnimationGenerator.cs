using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Pool;

namespace DH.Editor
{
    public class BattleAnimationGenerator : AssetPostprocessor
    {
        private const string RootPath = "Assets/GameAssets/Fighting/Enemy";
        private static Dictionary<string, string> cacheAssetPath = new();

        // private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
        //     string[] movedAssets,
        //     string[] movedFromAssetPaths)
        // {
        //     cacheAssetPath.Clear();
        //     foreach (var str in importedAssets) FilterValidAssetPath(str);
        //     foreach (var str in deletedAssets) FilterValidAssetPath(str);
        //     foreach (var str in movedAssets) FilterValidAssetPath(str);
        //
        //     foreach (var item in cacheAssetPath) ProcessAsset(item.Value);
        // }

        private static void FilterValidAssetPath(string assetPath)
        {
            if (!assetPath.StartsWith(RootPath, StringComparison.Ordinal) || !assetPath.EndsWith(".png")) return;

            var index = assetPath.LastIndexOf("_", StringComparison.Ordinal);
            var assetKey = assetPath.Substring(0, index);
            if (cacheAssetPath.ContainsKey(assetKey)) return;

            cacheAssetPath.Add(assetKey, assetPath);
        }

        private static void ProcessAsset(string importerAssetPath)
        {
            if (!importerAssetPath.StartsWith(RootPath, StringComparison.Ordinal)) return;
            var pool = ListPool<Sprite>.Get();
            var animClip = GetAnimationClip(importerAssetPath, out var animationPath);
            GetSpriteList(pool, importerAssetPath);
            animClip.frameRate = 15; // FPS
            var spriteBinding = new EditorCurveBinding();
            spriteBinding.type = typeof(SpriteRenderer);
            spriteBinding.path = "";
            spriteBinding.propertyName = "m_Sprite";
            var spriteKeyFrames = new ObjectReferenceKeyframe[pool.Count];
            for (var i = 0; i < pool.Count; i++)
            {
                spriteKeyFrames[i] = new ObjectReferenceKeyframe();
                spriteKeyFrames[i].time = (float)i / 15;
                spriteKeyFrames[i].value = pool[i];
            }

            AnimationUtility.SetObjectReferenceCurve(animClip, spriteBinding, spriteKeyFrames);
            ListPool<Sprite>.Release(pool);

            AssetDatabase.SaveAssetIfDirty(animClip);
        }

        private static void GetSpriteList(List<Sprite> sprites, string assetPath)
        {
            var folderName = Path.GetDirectoryName(assetPath);
            if (folderName == null) return;

            var assetFileName = Path.GetFileNameWithoutExtension(assetPath);
            var index = assetFileName.LastIndexOf("_", StringComparison.Ordinal);
            var animationName = assetFileName.Substring(0, index);

            var spriteIndex = 0;
            while (true)
            {
                var spritePath = Path.Combine(folderName, $"{animationName}_{spriteIndex:D2}.png");
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                if (!sprite) break;
                sprites.Add(sprite);
                spriteIndex++;
            }
        }

        private static AnimationClip GetAnimationClip(string assetImportPath, out string animationPath)
        {
            var folderName = Path.GetFileName(Path.GetDirectoryName(assetImportPath));
            var animationFolder = Path.Combine(RootPath, $"Animator/{folderName}");
            var assetFileName = Path.GetFileNameWithoutExtension(assetImportPath);
            var index = assetFileName.LastIndexOf("_", StringComparison.Ordinal);
            var animationName = assetFileName.Substring(0, index);
            animationPath = Path.Combine(animationFolder, $"{animationName}.anim");
            var animatorPath = Path.Combine(animationFolder, $"{folderName}.controller");
            var animator = AssetDatabase.LoadAssetAtPath<AnimatorController>(animatorPath);
            if (!animator) animator = AnimatorController.CreateAnimatorControllerAtPath(animatorPath);

            var animation = AssetDatabase.LoadAssetAtPath<AnimationClip>(animationPath);
            if (animation)
            {
                TryAddAnimationState(animation, animator, animationName.Replace($"{folderName}_", null));
                AssetDatabase.SaveAssetIfDirty(animator);
                return animation;
            }

            animation = new AnimationClip();
            AssetDatabase.CreateAsset(animation, animationPath);
            var settings = AnimationUtility.GetAnimationClipSettings(animation);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(animation, settings);
            TryAddAnimationState(animation, animator, animationName.Replace($"{folderName}_", null));
            AssetDatabase.SaveAssetIfDirty(animator);
            return animation;
        }

        private static void TryAddAnimationState(AnimationClip clip, AnimatorController animatorController,
            string stateName)
        {
            var states = animatorController.layers[0].stateMachine.states;
            var state = states.FirstOrDefault(x => x.state.name == stateName).state;
            if (state == null)
            {
                state = animatorController.layers[0].stateMachine.AddState(stateName);
                state.motion = clip;
                return;
            }

            state.motion = clip;
        }
    }
}