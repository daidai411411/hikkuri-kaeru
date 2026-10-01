using UnityEngine;
using UnityEngine.InputSystem;

public class TitleSceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private SceneTransition sceneTransition;

    void Update()
    {
        if (Keyboard.current.anyKey.wasPressedThisFrame ||
        Mouse.current.leftButton.wasPressedThisFrame ||
        Mouse.current.rightButton.wasPressedThisFrame ||
        Mouse.current.middleButton.wasPressedThisFrame
        )
        {
            sceneTransition.LoadScene("Menu");
        }
    }
}
