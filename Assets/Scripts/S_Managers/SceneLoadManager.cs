using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{
    public static SceneLoadManager Instance;

    void Awake()
    {
        // The newest copy wins: scene buttons reference the copy in their own scene, so it must be the one that stays alive
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(int sceneToLoad)
    {
        Debug.Log("Loading Scene " + sceneToLoad);
        SceneManager.LoadSceneAsync(sceneToLoad);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}