using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayEffectSound : MonoBehaviour
{
    public AudioManager audioManager;

    private void Start()
    {
        audioManager.PlaySound(1);
    }
}
