using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Data;
using DH.UIFramework.Observables;
using DHFramework;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace DH.Game
{
    public enum MixerGroupType
    {
        Bgm = 0,
        UI,
        Skill
    }
    
    public partial class AudioManager : ObservableObject
    {
        private static AudioManager _instance;

        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AudioManager();
                }

                return _instance;
            }
        }

        private const float AudioMaxVolume = 1;
        private static long AudioUnitId = 1;

        private bool audioMute = false;
        private bool musicMute = false;
        private GameObject audioRoot;
        private string curBgmPath = string.Empty;
        private GameObject audioPrefab;
        private readonly List<AudioSource> audioSourcesPool = new List<AudioSource>();
        private readonly List<AudioSource> usingAudioSources = new List<AudioSource>();
        private readonly List<AudioUnit> audioUnits = new List<AudioUnit>();
        private readonly Dictionary<string,AudioCacheUnit> cacheClips = new();
        private readonly List<AudioUnit> pendingList = new List<AudioUnit>();
        private readonly Dictionary<string, int> audioStat = new Dictionary<string, int>();
        private CancellationTokenSource cts = new CancellationTokenSource();

        public bool AudioMute
        {
            get => audioMute;
            set
            {
                if (!Set(ref audioMute,value)) return;
                // DHUnityUtil.PlayerPrefs.SetInt(nameof(AudioMute), value ? 1 : 0);
                DHUnityUtil.PlayerPrefs.Save();

                foreach (var unit in audioUnits)
                {
                    if (unit.music)
                    {
                        continue;
                    }
                    
                    unit.audioSource.mute = audioMute;
                }

            }
        }

        public bool MusicMute
        {
            get => musicMute;
            set
            {
                if (!Set(ref musicMute,value)) return;
                // DHUnityUtil.PlayerPrefs.SetInt(nameof(MusicMute), value ? 1 : 0);
                DHUnityUtil.PlayerPrefs.Save();
                foreach (var unit in audioUnits)
                {
                    if (!unit.music)
                    {
                        continue;
                    }
                    unit.audioSource.mute = musicMute;
                }
            }
        }

        public void Update(float deltaTime)
        {
            foreach (var item in audioUnits)
            {
                if (item.Update(deltaTime))
                {
                    continue;
                }
                DecAudioStat(item);
                pendingList.Add(item);
            }

            if (pendingList.Count == 0)
            {
                return;
            }

            foreach (var item in pendingList)
            {
                item.audioSource.gameObject.SetActive(false);
                usingAudioSources.Remove(item.audioSource);
                audioSourcesPool.Add(item.audioSource);
                ReferencePool.Release(item);
                audioUnits.Remove(item);
            }
            pendingList.Clear();
        }
        
        public void PlayMusic(string audioPath,float? fadeInTime = null, float? fadeOutTime = null)
        {
            if (curBgmPath == audioPath)
            {
                return;
            }
            
            PlayAudioWrap(audioPath, AudioUnitId++,true, fadeInTime,true,null, MixerGroupType.Bgm).Forget();
            StopAudio(curBgmPath, fadeInTime);
            curBgmPath = audioPath;
        }

        public AudioSource GetAudioSource(long id)
        {
            foreach (var item in audioUnits)
            {
                if (item.uniqueId != id)
                {
                    continue;   
                }
                return item.audioSource;
            }
            return null;
        }
        public void StopAudio(long id, float? fadeOutTime = null)
        {
            foreach (var item in audioUnits)
            {
                if (item.uniqueId != id)
                {
                    continue;   
                }
                
                item.Stop(fadeOutTime);
            }
        }

        public void StopAudio(string audioPath, float? fadeOutTime = null)
        {
            foreach (var item in audioUnits)
            {
                if (item.audioPath != audioPath)
                {
                    continue;   
                }
                
                item.Stop(fadeOutTime);
            }
        }
        
        public long PlayUIAudio(string audioPath, float? fadeInTime = null, bool loop =false)
        {
            audioPath = $"SFX_UI/{audioPath}";
            var id = PlayAudio(audioPath,fadeInTime,loop);
            return id;
        }

        public long PlayAudio(string audioPath, float? fadeInTime = null,
            bool loop =false,Vector3? position = null, MixerGroupType mixerGroupType = MixerGroupType.UI,
            string tag = "")
        {
            var id = AudioUnitId++;
            PlayAudioWrap(audioPath,id,  false, fadeInTime,loop,position, mixerGroupType, tag).Forget();
            return id;
        }
        
        public long PlayFightingAudio(string audioPath, float? fadeInTime = null,
            bool loop =false,Vector3? position = null, string tag = "")
        {
            audioPath = $"SFX_Battle/{audioPath}";
            return PlayAudio(audioPath, fadeInTime, loop, position, MixerGroupType.Skill, tag);
        }

        /// <summary>
        /// 挖矿的音效
        /// </summary>
        /// <param name="audioClip"></param>
        /// <param name="loop"></param>
        public void PlayAudioClip(AudioClip audioClip, bool loop = false)
        {
            AudioSource audioSource = GetAudioSourceFromPool();
            audioSource.clip = audioClip;
            audioSource.loop = false;
            audioSource.mute = audioMute;
            audioSource.outputAudioMixerGroup = AppGlobal.Instance.UIMixer;
            
            var unitId = AudioUnitId++;
            var unit = ReferencePool.Acquire<AudioUnit>();
            unit.audioPath = "digAudioPath";
            unit.audioSource = audioSource;
            unit.music = false;
            unit.uniqueId = unitId;
            unit.audioSource.loop = loop;
            unit.Play(0.1f);
            audioUnits.Add(unit);
        }

        private void StopAudioInternal(AudioUnit unit)
        {
            usingAudioSources.Remove(unit.audioSource);
            usingAudioSources.Remove(unit.audioSource);
        }

        private async UniTask<AudioClip> LoadAudioClip(string audioPath)
        {
            if (cacheClips.TryGetValue(audioPath, out var unit))
            {
                if (unit.instance)
                {
                    return unit.instance;
                }
                
                var tcs = AutoResetUniTaskCompletionSource<AudioClip>.Create();
                unit.pendingTasks.Add(tcs);
                return await tcs.Task;
            }
            
            {
                unit = ReferencePool.Acquire<AudioCacheUnit>();
                unit.pendingTasks = ListPool<AutoResetUniTaskCompletionSource<AudioClip>>.Get();
                cacheClips[audioPath] = unit;
                var result = await AssetsManager.LoadAssetAsync<AudioClip>(audioPath,cts.Token);
                unit.instance = result;
                foreach (var item in unit.pendingTasks)
                {
                    item.TrySetResult(result);
                }
                return result;
            }
        }

        private AudioMixerGroup GetAudioMixerGroup(MixerGroupType type)
        {
            if (type == MixerGroupType.Bgm)
            {
                return AppGlobal.Instance.BgmMixer;
            }
            else if (type == MixerGroupType.Skill)
            {
                return AppGlobal.Instance.SkillMixer;
            }
            return AppGlobal.Instance.UIMixer;
        }

        private async UniTaskVoid PlayAudioWrap(string audioPath,long unitId,bool isMusic, 
            float? fadeInTime,bool loop,Vector3? position, 
            MixerGroupType mixerGroupType = MixerGroupType.UI, string tag = "")
        {
            var audioClip = await LoadAudioClip(audioPath);
            if (audioClip)
            {
                AudioSource audioSource = GetAudioSourceFromPool();
                audioSource.clip = audioClip;
                audioSource.loop = false;
                audioSource.mute = isMusic ? musicMute : audioMute;
                audioSource.outputAudioMixerGroup = GetAudioMixerGroup(mixerGroupType);

                var unit = ReferencePool.Acquire<AudioUnit>();
                unit.audioPath = audioPath;
                unit.audioSource = audioSource;
                unit.music = isMusic;
                unit.uniqueId = unitId;
                unit.audioSource.loop = loop;
                unit.Tag = tag;
                unit.Play(fadeInTime);
                audioUnits.Add(unit);
                AddAudioStat(unit);

                if (position.HasValue)
                {
                    unit.audioSource.transform.position = position.Value;
                    unit.audioSource.spatialBlend = 1;
                }
                else
                {
                    unit.audioSource.spatialBlend = 0;
                }
            }
        }

        private AudioSource GetAudioSourceFromPool()
        {
            AudioSource audioSource;
            if (audioSourcesPool.Count > 0)
            {
                audioSource = audioSourcesPool[0];
                audioSourcesPool.RemoveAt(0);
                audioSource.gameObject.SetActive(true);
            }
            else
            {
                var obj = Object.Instantiate(audioPrefab, AppGlobal.Instance.AudioRoot.transform, false);
                audioSource = obj.GetComponent<AudioSource>();
            }
            usingAudioSources.Add(audioSource);
            return audioSource;
        }

        public void Init()
        {
            audioRoot = AppGlobal.Instance.AudioRoot;
            // audioMute = DHUnityUtil.PlayerPrefs.GetInt(nameof(AudioMute)) == 1;
            // musicMute = DHUnityUtil.PlayerPrefs.GetInt(nameof(MusicMute)) == 1;
            // audioMute = !DataCenter.userinfo.EffectState;
            // musicMute = !DataCenter.userinfo.MusicState;
            audioMute = DHUnityUtil.PlayerPrefs.GetInt(GameConst.UserInfoCode.EffectState, 1) == 0;
            musicMute = DHUnityUtil.PlayerPrefs.GetInt(GameConst.UserInfoCode.MusicState, 1) == 0;
            audioPrefab = AssetsManager.LoadAssetSync<GameObject>("Audio/Template");
        }

        private void AddAudioStat(AudioUnit unit)
        {
            if(string.IsNullOrEmpty(unit.Tag))return;
            if (audioStat.TryGetValue(unit.Tag, out int value))
            {
                value += 1;
            }
            else
            {
                value = 1;
            }

            audioStat[unit.Tag] = value;
        }

        private void DecAudioStat(AudioUnit unit)
        {
            if(string.IsNullOrEmpty(unit.Tag))return;
            if (audioStat.TryGetValue(unit.Tag, out int value))
            {
                value -= 1;
                value = value < 0 ? 0 : value;
                audioStat[unit.Tag] = value;
            }
        }

        public int GetAudioCount(string tag)
        {
            if (audioStat.TryGetValue(tag, out int value))
            {
                return value;
            }

            return 0;
        }
        
        public void ReleaseUnusedClip()
        {
            var hashSet = new HashSet<AudioClip>();
            var pendingRemove = new List<string>();
            foreach (var item in audioUnits)
            {
                if (!item.audioSource.clip)
                {
                    continue;
                }
                
                hashSet.Add(item.audioSource.clip);
            }

            foreach (var item in cacheClips)
            {
                if (!item.Value.instance)
                {
                    continue;
                }
                
                if (!hashSet.Contains(item.Value.instance))
                {
                    pendingRemove.Add(item.Key);
                    AssetsManager.Release(item.Value.instance);
                }
            }

            foreach (var removeKey in pendingRemove)
            {
                ReferencePool.Release(cacheClips[removeKey]);
                cacheClips.Remove(removeKey);
            }
        }

        public void Release()
        {
            audioSourcesPool.Clear();
            usingAudioSources.Clear();

            foreach (var item in audioUnits)
            {
                ReferencePool.Release(item);
            }
            audioUnits.Clear();
            pendingList.Clear();

            AssetsManager.Release(audioPrefab);
            audioPrefab = null;
            foreach (var item in cacheClips)
            {
                ReferencePool.Release(item.Value);
                if (!item.Value.instance)
                {
                    continue;
                }
                
                AssetsManager.Release(item.Value.instance);
            }
            cacheClips.Clear();
        }
    }
}