using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InputManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    void Awake()
    {
        inputActions.Disable();
        if(SceneManager.GetActiveScene().name.Contains("Stage"))
        {
            inputActions.FindActionMap("Stage").Enable();
        }
        else
        {
            inputActions.FindActionMap(SceneManager.GetActiveScene().name).Enable();
        }
    }
}
