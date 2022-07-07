using UnityEngine;
using UnityEngine.Audio;
using System.IO;

[DisallowMultipleComponent]
public class SettingsManager : MonoBehaviour {
    public SettingsData Settings { get; private set; }
    SettingsData tempSettings;
    [SerializeField] AudioMixer audioMixer;

    public void Init() {
        Settings = SettingsData.LoadFromFile();
        tempSettings = Settings.Copy();
    }

    void Start() {
        Apply(Settings); // AudioMixer needs to be loaded!
    }

    void Apply(SettingsData settings) {
        audioMixer.SetFloat("MasterVolume", settings.masterVolume);
        audioMixer.SetFloat("MusicVolume", settings.musicVolume);
        audioMixer.SetFloat("EffectsVolume", settings.effectsVolume);

        Screen.SetResolution(settings.resolution.width, settings.resolution.height, settings.fullScreenMode);
    }

    /// <summary>
    /// Save and use tempSettings
    /// </summary>
    public void Save() {
        Settings = tempSettings;
        Settings.SaveToFile();
        Apply(Settings);
    }
    
    /// <summary>
    /// Discard tempSettings and use normal settings again
    /// </summary>
    public void Revert() {
        Apply(Settings);
        tempSettings = Settings.Copy();
    }

    public void OnMasterVolumeChanged(float volume) {
        tempSettings.masterVolume = volume;
        Apply(tempSettings);
        // TODO: Play sound
    }

    public void OnMusicVolumeChanged(float volume) {
        tempSettings.musicVolume = volume;
        Apply(tempSettings);
    }

    public void OnEffectsVolumeChanged(float volume) {
        tempSettings.effectsVolume = volume;
        Apply(tempSettings);
        // TODO: Play sound
    }

    public void OnWindowModeChanged(FullScreenMode fullScreenMode) {
        tempSettings.fullScreenMode = fullScreenMode;
    }

    public void OnResolutionChanged(Resolution resolution) {
        tempSettings.resolution = resolution;
    }

    public class SettingsData {
        public Resolution resolution;
        public FullScreenMode fullScreenMode;

        public float masterVolume;
        public float musicVolume;
        public float effectsVolume;

        public SettingsData() {
            resolution = Screen.currentResolution;
            fullScreenMode = FullScreenMode.ExclusiveFullScreen;

            masterVolume = -40f;
            musicVolume = -40f;
            effectsVolume = -40f;
        }

        public SettingsData Copy() {
            return new SettingsData {
                resolution = resolution,
                fullScreenMode = fullScreenMode,
                masterVolume = masterVolume,
                musicVolume = musicVolume,
                effectsVolume = effectsVolume
            };
        }

        static string filePath = "/StreamingAssets/settings.json";
        public static SettingsData LoadFromFile() {
            SettingsData settings;
            string filePath = Application.dataPath + SettingsData.filePath;

            if (File.Exists(filePath)) {
                string dataAsJson = File.ReadAllText(filePath);
                settings = JsonUtility.FromJson<SettingsData>(dataAsJson);
            } else {
                settings = new SettingsData();
                settings.SaveToFile();
            }

            return settings;
        }

        public void SaveToFile() {
            string dataAsJson = JsonUtility.ToJson(this);
            string filePath = Application.dataPath + SettingsData.filePath;

            File.WriteAllText(filePath, dataAsJson);
        }
    }
}