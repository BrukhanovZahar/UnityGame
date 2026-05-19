using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class EscMenu : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private GameObject ui;

    [Header("Slider References")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider effectsSlider;

    [Header("Mixer Group Names")]
    [SerializeField] private string musicGroupName = "Music";
    [SerializeField] private string effectsGroupName = "Effects";

    private bool on = false;

    private void Start()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (effectsSlider != null)
            effectsSlider.onValueChanged.AddListener(OnEffectsVolumeChanged);

        LoadVolumes();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        ui.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !GlobalContext.End)
            Switch();
    }

    private void Switch()
	{
        on = !on;
        ui.SetActive(on);
        GlobalContext.Pause = on;
        Cursor.visible = on;

        if (on)
		{
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
		}
		else
		{
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
		}
    }

    public void OnMusicVolumeChanged(float sliderValue)
    {
        audioMixer.SetFloat(musicGroupName, sliderValue);
        PlayerPrefs.SetFloat(musicGroupName, sliderValue);
        PlayerPrefs.Save();
    }

    public void OnEffectsVolumeChanged(float sliderValue)
    {
        audioMixer.SetFloat(effectsGroupName, sliderValue);
        PlayerPrefs.SetFloat(effectsGroupName, sliderValue);
        PlayerPrefs.Save();
    }

    private void LoadVolumes()
    {
        float savedMusicVolume = PlayerPrefs.GetFloat(musicGroupName, 0);
        musicSlider.value = savedMusicVolume;
        OnMusicVolumeChanged(savedMusicVolume);

        float savedEffectsVolume = PlayerPrefs.GetFloat(effectsGroupName, 0);
        effectsSlider.value = savedEffectsVolume;
        OnEffectsVolumeChanged(savedEffectsVolume);
    }
}
