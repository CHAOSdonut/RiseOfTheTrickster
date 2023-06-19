using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource _musicSource, _effectsSource;
    [SerializeField] private AudioClip[] buttonSounds;

    private float stoppedTime = 0f;
    
    private static SoundManager _instance;
    
    public static SoundManager Instance
    {
        get
        {
            // instance already exists -> return it right away
            if(_instance) return _instance;

            // look in the scene for one
            _instance = FindObjectOfType<SoundManager>();

            // found one -> return it
            if(_instance) return _instance;

            // otherwise create a new one from scratch
            _instance = new GameObject(nameof(SoundManager), typeof(AudioSource)).AddComponent<SoundManager>();
            return _instance;
        }
    }
    
    private void Awake()
    {
        // ensure singleton
        if(_instance && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        // lazy initialization of AudioSource component
        if(!_musicSource) 
        {
            if(!TryGetComponent<AudioSource>(out _musicSource))
            {
                _musicSource = gameObject.AddComponent<AudioSource>();
            }
        }
        // lazy initialization of AudioSource component
        if(!_effectsSource) 
        {
            if(!TryGetComponent<AudioSource>(out _effectsSource))
            {
                _effectsSource = gameObject.AddComponent<AudioSource>();
            }
        }



        DontDestroyOnLoad(transform.gameObject);
    }
    private void Start()
    {
        if (PlayerPrefs.GetInt("Sound") == 0)
        {
            _musicSource.mute = true;
            _effectsSource.mute = true;
        }
    }

    public void PlaySound(AudioClip clip)
    {
        _effectsSource.PlayOneShot(clip);
    }

    public void PlaySoundWithVolume(AudioClip clip, float volume)
    {
        _effectsSource.PlayOneShot(clip, volume);
    }

    public void SetMusic(AudioClip clip)
    {
        _musicSource.clip = clip;
        _musicSource.Play();
    }

    public void stopMusic()
    {
        _musicSource.Stop();
    }

    public void ToggleMusic() => _musicSource.mute = !_musicSource.mute;
    
    public void ToggleEffects() => _effectsSource.mute = !_effectsSource.mute;
    
    public void ToggleAllSounds()
    {
        _musicSource.mute = !_musicSource.mute;
        _effectsSource.mute = !_effectsSource.mute;
    }

    public void SetMusicVolume(float volume) => _musicSource.volume = volume;
    
    public void SetEffectsVolume(float volume) => _effectsSource.volume = volume;

    public void PlayButtonSound() => _effectsSource.PlayOneShot(buttonSounds[Random.Range(0, buttonSounds.Length)]);

}
