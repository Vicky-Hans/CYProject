using System;
using DHFramework;
using UnityEngine;

namespace DH.Game
{
    public partial class AudioManager
    {
        private enum EAudioState
        {
            None,
            FadeIn,
            Playing,
            FadeOut,
        }

        private class AudioUnit : IReference
        {
            public AudioSource audioSource;
            public EAudioState state;
            public bool music;
            public long uniqueId;
            public string audioPath;

            private float fadeInTime;
            private float fadeOutTime;
            private float timer;
            
            public string Tag { get; set; }

            public void Play(float? fadeTime = null)
            {
                if (fadeTime.HasValue)
                {
                    state = EAudioState.FadeIn;
                    timer = 0;
                    fadeInTime = fadeTime.Value;
                    audioSource.volume = 0;
                    audioSource.Play();
                }
                else
                {
                    state = EAudioState.Playing;
                    audioSource.volume = AudioMaxVolume;
                    audioSource.Play();
                }
            }

            public void Stop(float? fadeTime = null)
            {
                if (fadeTime.HasValue)
                {
                    state = EAudioState.FadeOut;
                    timer = 0;
                    fadeOutTime = fadeTime.Value;
                }
                else
                {
                    state = EAudioState.None;
                    audioSource.Stop();
                }
            }

            public bool Update(float deltaTime)
            {
                switch (state)
                {
                    case EAudioState.None:
                        break;

                    case EAudioState.FadeIn:
                    {
                        timer += deltaTime;
                        if (timer > fadeInTime)
                        {
                            state = EAudioState.Playing;
                            audioSource.volume = 1;
                            break;
                        }

                        audioSource.volume = Mathf.Lerp(0, 1, timer / fadeInTime);
                    }
                        break;

                    case EAudioState.Playing:
                        if (!audioSource.isPlaying && audioSource.time > 0f)
                        {
                            break;
                        }
                        if (!audioSource.isPlaying)
                        {
                            return false;
                        }

                        break;

                    case EAudioState.FadeOut:
                    {
                        timer += deltaTime;
                        if (timer > fadeOutTime)
                        {
                            state = EAudioState.None;
                            audioSource.volume = 0;
                            audioSource.Stop();
                            return false;
                        }

                        audioSource.volume = Mathf.Lerp(1, 0, timer / fadeOutTime);
                    }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return true;
            }

            public void Clear()
            {
                state = EAudioState.None;
                audioSource = null;
                music = false;
                audioPath = null;
                uniqueId = 0;
            }
        }
    }
}