using UnityEngine;

public class ReturnToMenu : MonoBehaviour
{
    [SerializeField] private SceneTransition sceneTransition;
    public void OnClick()
    {
        sceneTransition.LoadScene("Menu");
    }
}
