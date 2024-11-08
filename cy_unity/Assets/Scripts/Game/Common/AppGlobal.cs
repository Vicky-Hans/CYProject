using System.Collections;
using System.Collections.Generic;
using DH.Game;
using DHFramework;
using UnityEngine;
using UnityEngine.Audio;

public class AppGlobal : Singleton<AppGlobal>
{
    public MonoBehaviour GlobalMono;
    public List<Transform> UIRoots;
    public Camera BaseCamera;
    public Camera UICamera;
    public bool IsDebug;
    public bool IsBattleDebug;
    public GameObject AudioRoot;
    public Transform audioListener;
    public AudioMixerGroup BgmMixer;
    public AudioMixerGroup UIMixer;
    public AudioMixerGroup SkillMixer;
    public Transform UIEffectRoot;
    public Canvas UICanvas;
    
    public Coroutine StartCoroutine(string methodName)
    {
        return GlobalMono.StartCoroutine(methodName);
    }
    
    public Coroutine StartCoroutine(IEnumerator routine)
    {
        return GlobalMono.StartCoroutine(routine);
    }
}