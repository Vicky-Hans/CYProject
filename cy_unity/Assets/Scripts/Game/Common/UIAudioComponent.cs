using DH.Game;
using UnityEngine;

public class UIAudioComponent : MonoBehaviour
{
    [AssetPath] public string audioPath;

    private void Start()
    {
        AudioManager.Instance.PlayAudio(audioPath);
    }
}
