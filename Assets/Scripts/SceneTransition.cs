using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;


public class SceneTransition : MonoBehaviour
{
    [SerializeField] private Image overlayImage;
    private bool isTransitioning = false;
    private void Awake()
    {
        Color overlayColor = overlayImage.color;
        overlayColor.a = 0f;
        overlayImage.color = overlayColor;
        overlayImage.gameObject.SetActive(false);
    }
    public void LoadScene(string sceneName = null)
    {
        if (isTransitioning) return;
        isTransitioning = true;
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName = null)
    {
        Debug.Log("LoadScene: " + sceneName);
        if (string.IsNullOrEmpty(sceneName))
        {
            yield break;
        }
        var asyncload = SceneManager.LoadSceneAsync(sceneName);
        asyncload.allowSceneActivation = false;
        yield return StartCoroutine(TransitionEffect());
        while(asyncload.progress < 0.9f)
        {
            yield return null;
        }
        isTransitioning = false;
        asyncload.allowSceneActivation = true;
    }

    private IEnumerator TransitionEffect()
    {
        overlayImage.gameObject.SetActive(true);
        Color overlayColor = overlayImage.color;
        overlayColor.a = 0f;
        overlayImage.color = overlayColor;
        for(int i = 0; i < 10; i++)
        {
            overlayColor.a += 0.1f;
            overlayImage.color = overlayColor;
            yield return null;
        }
    }
}
