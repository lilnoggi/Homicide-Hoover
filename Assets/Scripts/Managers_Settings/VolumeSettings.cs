using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [Header("References")]
    public AudioMixer mainMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Start()
    {
        // Init sliders with current mixer values
        if (PlayerPrefs.HasKey("MusicVol"))
            LoadVolume();
        else
        {
            SetMusicVolume(0.75f);
            SetSFXVolume(0.75f);
        }
    }

    // Call this method from the Music Slider -> OnValueChanged
    public void SetMusicVolume(float value)
    {
        // Use Log10 to convert decibels to linear numbers
        float volume = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;

        mainMixer.SetFloat("MusicVol", volume);

        // Save for next time
        PlayerPrefs.SetFloat("MusicVol", value);
    }

    // Call this method from the SFX Slider -> OnValueChanged
    public void SetSFXVolume(float value)
    {
        float volume = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;

        mainMixer.SetFloat("SFXVol", volume);

        PlayerPrefs.SetFloat("SFXVol", value);
    }

    private void LoadVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVol");
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVol");

        SetMusicVolume(musicSlider.value);
        SetSFXVolume(sfxSlider.value);
    }
}
