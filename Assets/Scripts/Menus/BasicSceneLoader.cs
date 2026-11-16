using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicSceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    public void LoadScene()
    {
        Debug.Log("Loading: "+ sceneToLoad);
        SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single);
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
