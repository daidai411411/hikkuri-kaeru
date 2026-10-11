using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private GameObject overlay;
    [SerializeField] private GameObject retryButton;
    [SerializeField] private GameObject returnToMenuButton;
    [SerializeField] private GameObject continueButton;
    private InputAction pauseAction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        pauseAction = inputActions.FindAction("Stage/Pause");
        // 開発用シーンにはInputManagerが無いため、ここで有効にする
        pauseAction.Enable();
    }

    void Update()
    {
        if(pauseAction.WasPressedThisFrame())
        {
            Time.timeScale = 0f;
            overlay.SetActive(true);
            retryButton.SetActive(true);
            returnToMenuButton.SetActive(true);
            continueButton.SetActive(true);
            //再開の処理はContinueButton.csのOnClick()に書く
        }
    }
}
