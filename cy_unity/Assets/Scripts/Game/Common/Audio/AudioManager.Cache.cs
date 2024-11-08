using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DHFramework;
using UnityEngine;
using UnityEngine.Pool;

namespace DH.Game
{
    public partial class AudioManager
    {
        public class AudioCacheUnit : IReference
        {
            public List<AutoResetUniTaskCompletionSource<AudioClip>> pendingTasks;
            public int referenceCount;
            public AudioClip instance;

            public void Clear()
            {
                if (pendingTasks != null)
                {
                    ListPool<AutoResetUniTaskCompletionSource<AudioClip>>.Release(pendingTasks);
                    pendingTasks = null;
                }

                instance = null;
            }
        }
    }
}