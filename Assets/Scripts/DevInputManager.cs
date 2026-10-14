using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DevInputManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string scereneName;
    void Awake()
    {
        inputActions.Disable();
        inputActions.FindActionMap(scereneName).Enable();
    }
}
