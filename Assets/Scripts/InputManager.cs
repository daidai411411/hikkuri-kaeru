using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InputManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    void Awake()
    {
        inputActions.Disable();
        inputActions.FindActionMap(SceneManager.GetActiveScene().name).Enable();
    }
}
