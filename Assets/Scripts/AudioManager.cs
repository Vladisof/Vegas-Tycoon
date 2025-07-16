using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioSource soundSource;

    public List<AudioClip> soundClips;

    public Image musicToggleImage;

    public Sprite musicOnSprite;
    public Sprite musicOffSprite;

    private bool isSoundOn = true;
    private bool isMusicOn = true;

    private void Start()
    {
        isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        isSoundOn = isMusicOn;

        if (isMusicOn)
        {
            musicSource.Play();
        }
        else
        {
            musicSource.Pause();
        }

        UpdateMusicIcon();
    }
    
    public void PlayButtonClickSound()
    {
        if (isSoundOn)
        {
            soundSource.PlayOneShot(soundClips[0]);
        }
    }

    public void PlaySound(int index)
    {
        if (isSoundOn && index >= 0 && index < soundClips.Count)
        {
            soundSource.PlayOneShot(soundClips[index]);
        }
    }

    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        isSoundOn = isMusicOn;

        if (isMusicOn)
        {
            musicSource.Play();
        }
        else
        {
            musicSource.Pause();
        }

        PlayerPrefs.SetInt("MusicOn", isMusicOn ? 1 : 0);
        PlayerPrefs.Save();

        UpdateMusicIcon();
    }

    private void UpdateMusicIcon()
    {
        if (musicToggleImage != null)
        {
            musicToggleImage.sprite = isMusicOn ? musicOnSprite : musicOffSprite;
        }
    }
}
