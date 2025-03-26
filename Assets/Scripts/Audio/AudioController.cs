using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AudioController : MonoBehaviour
{
    // Singleton instance
    public static AudioController Instance { get; private set; }
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    
    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip battleMusic;
    [SerializeField] private AudioClip demonstrationMusic;
    
    [Header("UI Sounds")]
    [SerializeField] private AudioClip buttonClick;
    
    [Header("Achievement Sounds")]
    [SerializeField] private AudioClip achievementComplete;
    [SerializeField] private AudioClip objectiveComplete;
    
    [Header("Level Sounds")]
    [SerializeField] private AudioClip levelComplete;

    [Header("Shop Sounds")]
    [SerializeField] private AudioClip buyItem;

    [Header("Battle Sounds")]
    [SerializeField] private AudioClip battleVictory;
    [SerializeField] private AudioClip battleLoss;
    [SerializeField] private AudioClip hit;
    [SerializeField] private AudioClip powerUp;

    [Header("Demonstration Sounds")]
    [SerializeField] private AudioClip answerCorrect;
    [SerializeField] private AudioClip answerIncorrect;
    [SerializeField] private AudioClip npcInteract;

    [Header("Menu Sounds")]
    [SerializeField] private AudioClip menuOpen;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 0.7f;

    [Header("Sliders")]
    [SerializeField] private Slider musicVolumeSlider; // Slider for music volume
    [SerializeField] private Slider sfxVolumeSlider;   // Slider for SFX volume
    
    private bool isDemonstrationMusicPlaying;
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize audio sources if not set in inspector
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
            
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }

            // Initialize sliders if assigned
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = musicVolume;
                musicVolumeSlider.onValueChanged.AddListener(SetMusicVolumeFromSlider);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = sfxVolume;
                sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolumeFromSlider);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Play background music
    public void PlayBackgroundMusic()
    {
        isDemonstrationMusicPlaying = false;
        PlayMusic(backgroundMusic);
    }

    // Play demonstration music
    public void PlayDemonstrationMusic()
    {
        isDemonstrationMusicPlaying = true;
        PlayMusic(demonstrationMusic);
    }
    
    // Play battle music
    public void PlayBattleMusic()
    {
        isDemonstrationMusicPlaying = false;
        PlayMusic(battleMusic);
    }

    
    // Generic music player with optional crossfade
    public void PlayMusic(AudioClip music, bool fade = true, float fadeTime = 1.0f)
    {
        if (music == null) return;
        
        if (fade && musicSource.isPlaying && musicSource.clip != music)
        {
            StartCoroutine(CrossFadeMusic(music, fadeTime));
        }
        else
        {
            musicSource.clip = music;
            if(isDemonstrationMusicPlaying)
            {
                musicSource.volume = musicVolume * 0.5f;
            }
            else
            {
                musicSource.volume = musicVolume;
            }
            musicSource.Play();
        }
    }
    
    // Fade between music tracks
    private IEnumerator CrossFadeMusic(AudioClip newClip, float fadeTime)
    {
        float startVolume = musicSource.volume;
        float timer = 0;
        
        // Fade out current music
        while (timer < fadeTime / 2)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0, timer / (fadeTime / 2));
            yield return null;
        }
        
        // Switch clips
        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.volume = 0;
        musicSource.Play();
        
        timer = 0;
        
        // Fade in new music
        while (timer < fadeTime / 2)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0, musicVolume, timer / (fadeTime / 2));
            yield return null;
        }
        
        // Ensure final volume is correct
        musicSource.volume = musicVolume;
    }

    public void musicOff()
    {
        ToggleMusic(false);
        ToggleSFX(false);
        
    }

    public void musicOn()
    {
        ToggleMusic(true);
        ToggleSFX(true);
    }
    
    // Play a sound effect
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
  
        sfxSource.PlayOneShot(clip, sfxVolume);
    }
    
    #region Sound Effect Methods
    
    public void PlayButtonClick()
    {
        PlaySFX(buttonClick);
    }

    public void PlayNpcInteract()
    {
        PlaySFX(npcInteract);
    }

    public void PlayPowerUp()
    {
        PlaySFX(powerUp);
    }

    public void PlayMenuOpen()
    {
        PlaySFX(menuOpen);
    }
    
    public void PlayLevelComplete()
    {
        PlaySFX(levelComplete);
    }

    public void PlayLevelFailed()
    {
        PlaySFX(battleLoss);
    }

    public void PlayAnswerCorrect()
    {
        PlaySFX(answerCorrect);
    }

    public void PlayAnswerIncorrect()
    {
        PlaySFX(answerIncorrect);
    }
    
    public void PlayAchievementComplete()
    {
        PlaySFX(achievementComplete);
    }

    public void PlayBuyItem()
    {
        PlaySFX(buyItem);
    }
    
    public void PlayObjectiveComplete()
    {
        PlaySFX(objectiveComplete);
    }
    
    public void PlayBattleVictory()
    {
        PlaySFX(battleVictory);
    }
    
    public void PlayBattleLoss()
    {
        PlaySFX(battleLoss);
    }
    
    public void PlayHit()
    {
        PlaySFX(hit);
    }
    
    
    #endregion
    
    #region Volume Controls
    
    // Method to set music volume from slider
    public void SetMusicVolumeFromSlider(float value)
    {
        SetMusicVolume(value);
    }

    // Method to set SFX volume from slider
    public void SetSFXVolumeFromSlider(float value)
    {
        SetSFXVolume(value);
    }

    // Existing methods for setting volume
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }
    
    public float GetMusicVolume()
    {
        return musicVolume;
    }
    
    public float GetSFXVolume()
    {
        return sfxVolume;
    }
    
    public void ToggleMusic(bool isOn)
    {
        musicSource.mute = !isOn;
    }
    
    public void ToggleSFX(bool isOn)
    {
        sfxSource.mute = !isOn;
    }
    
    #endregion
}