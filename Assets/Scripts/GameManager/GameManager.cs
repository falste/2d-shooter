using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(SettingsManager))]
public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }
    public GameStateData GameState { get; private set; }

    void Awake()
    {
        if (Instance == null) {
            Instance = this;
        } else {
            Debug.LogError("Multiple GameManager instances detected!");
        }

        GetComponent<SettingsManager>().Init();

        SceneManager.LoadScene("Menu", LoadSceneMode.Additive);
        StartCoroutine(LoadSceneAsync("TitleScreenBackground"));
    }

    public void PlaySave(int index)
    {
        string fileName = "save" + index + ".json";
        GameState = GameStateData.LoadFromFile(fileName);
        StartCoroutine(UnloadSceneAsync("TitleScreenBackground"));
        StartCoroutine(LoadSceneAsync("Game"));
    }

    public void StopGame()
    {
        int saveIndex = 0;
        string fileName = "save" + saveIndex + ".json";
        GameState.SaveToFile(fileName);
        StartCoroutine(UnloadSceneAsync("Game"));
        StartCoroutine(LoadSceneAsync("TitleScreenBackground"));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone) {
            // Advance loading bar here

            yield return null;
        }
    }

    IEnumerator UnloadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.UnloadSceneAsync(sceneName);

        while (!asyncLoad.isDone) {
            yield return null;
        }
    }
}
