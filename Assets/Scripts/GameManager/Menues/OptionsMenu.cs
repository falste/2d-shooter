using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// TODO: Allow double click to reset sliders
// TODO: Allow entering value for sliders

[DisallowMultipleComponent]
public class OptionsMenu : MonoBehaviour {
    SettingsManager settingsManager;
    SettingsManager.SettingsData settings;

    Resolution[] resolutions;

    [SerializeField] Slider masterVolumeSlider;
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Slider effectsVolumeSlider;
    [SerializeField] Dropdown resolutionsDropdown;
    [SerializeField] Dropdown windowModeDropdown;

    void Awake() {
        settingsManager = GameManager.Instance.GetComponent<SettingsManager>();

        // Set resolution options
        resolutions = Screen.resolutions;
        resolutionsDropdown.ClearOptions();
        List<string> options = new List<string>();
        string option;
        for (int i = 0; i < resolutions.Length; i++) {
            option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
        }
        resolutionsDropdown.AddOptions(options);
    }

    void OnEnable() {
        // Set initial values
        settings = settingsManager.Settings;

        // Sliders
        masterVolumeSlider.value = settings.masterVolume;
        musicVolumeSlider.value = settings.musicVolume;
        effectsVolumeSlider.value = settings.effectsVolume;

        // Resolution Dropdown
        int resolutionIndex = -1;
        for (int i = 0; i < resolutions.Length; i++) {
            if (CompResolutions(resolutions[i], settings.resolution)) {
                resolutionIndex = i;
                break;
            }
        }
        if (resolutionIndex != -1) {
            resolutionsDropdown.value = resolutionIndex;
        }
        resolutionsDropdown.RefreshShownValue();

        // WindowMode Dropdown
        switch (settings.fullScreenMode) {
            case FullScreenMode.ExclusiveFullScreen:
                windowModeDropdown.value = 0;
                break;
            case FullScreenMode.FullScreenWindow:
                windowModeDropdown.value = 1;
                break;
            case FullScreenMode.Windowed:
                windowModeDropdown.value = 2;
                break;
        }
        windowModeDropdown.RefreshShownValue();
    }

    #region SettingsManager communication functions
    public void Save() {
        settingsManager.Save();
    }

    public void Revert() {
        settingsManager.Revert();
    }

    public void OnMasterVolumeChanged(float volume) {
        settingsManager.OnMasterVolumeChanged(volume);
    }

    public void OnMusicVolumeChanged(float volume) {
        settingsManager.OnMusicVolumeChanged(volume);
    }

    public void OnEffectsVolumeChanged(float volume) {
        settingsManager.OnEffectsVolumeChanged(volume);
    }

    public void OnResolutionChanged(int index) {
        settingsManager.OnResolutionChanged(resolutions[index]);
    }

    public void OnWindowModeChanged(int index) {
        FullScreenMode windowMode;
        switch (index) {
            case 0:
                windowMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case 1:
                windowMode = FullScreenMode.FullScreenWindow;
                break;
            case 2:
                windowMode = FullScreenMode.Windowed;
                break;
            default:
                throw new System.Exception();
        }
        settingsManager.OnWindowModeChanged(windowMode);
    }
    #endregion

    bool CompResolutions(Resolution a, Resolution b) {
        if (a.width == b.width && a.height == b.height) {
            return true;
        } else {
            return false;
        }
    }
}