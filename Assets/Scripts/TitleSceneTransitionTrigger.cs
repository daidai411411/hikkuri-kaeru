using UnityEngine;
using UnityEngine.InputSystem;

public class TitleSceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private SceneTransition sceneTransition;
    [SerializeField] private InputActionAsset inputActions;
    private InputAction startAction;
    void Awake()
    {
        startAction = inputActions.FindAction("Title/Start");
    }
    void Update()
    {
        if (startAction.WasPressedThisFrame())
        {
            sceneTransition.LoadScene("Menu");
        }
    }
}
