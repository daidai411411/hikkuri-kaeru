using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

// 触れたカエルを死亡させる（池・いばら）。死亡したら今のシーンを最初から読み直す
public class KillZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            ReloadCurrentScene();
        }
    }

    private void ReloadCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
#if UNITY_EDITOR
        // 開発用シーンはBuild Settingsに入っておらずSceneManager.LoadSceneで読めないため、エディタではパスで読み直す
        EditorSceneManager.LoadSceneInPlayMode(currentScene.path, new LoadSceneParameters(LoadSceneMode.Single));
#else
        SceneManager.LoadScene(currentScene.buildIndex);
#endif
    }
}
