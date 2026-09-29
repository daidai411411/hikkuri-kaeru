using UnityEngine;
using UnityEngine.InputSystem;

public class TitleSceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private SceneTransition sceneTransition;

    void Update()
    {
        if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            sceneTransition.LoadScene("Menu");
        }
    }
}
