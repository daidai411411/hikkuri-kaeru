using UnityEngine;

public class ReturnToTitle : MonoBehaviour
{
    [SerializeField] private SceneTransition sceneTransition;
    public void OnClick()
    {
        sceneTransition.LoadScene("Title");
    }
}
