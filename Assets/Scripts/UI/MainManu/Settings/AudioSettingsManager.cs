using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class AudioSettingsManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("UI Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    void Start()
    {
        float master = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        masterSlider.value = master;
        musicSlider.value = music;
        sfxSlider.value = sfx;

        SetMasterVolume(master);
        SetMusicVolume(music);
        SetSFXVolume(sfx);

        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMasterVolume(float value)
    {
        if (value == 0)
        {
            audioMixer.SetFloat("MasterVolume", -80); // Mute
        }
        else
        {
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
        }

        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        if (value == 0)
        {
            audioMixer.SetFloat("MusicVolume", -80); // Mute
        }
        else
        {
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
        }

        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        if(value == 0)
        {
            audioMixer.SetFloat("SFXVolume", -80); // Mute
        }
        else
        {
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
        }

        PlayerPrefs.SetFloat("SFXVolume", value);
    }
}
