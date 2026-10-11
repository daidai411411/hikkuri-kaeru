using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private GameObject overlay;
    [SerializeField] private GameObject overlay2;
    [SerializeField] private GameObject retryButton;
    [SerializeField] private GameObject returnToMenuButton;
    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject pauseText;
    private InputAction pauseAction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        pauseAction = inputActions.FindAction("Stage/Pause");
    }

    void Update()
    {
        if(pauseAction.WasPressedThisFrame())
        {
            Time.timeScale = 0f;
            overlay.SetActive(true);
            overlay2.SetActive(true);
            retryButton.SetActive(true);
            returnToMenuButton.SetActive(true);
            continueButton.SetActive(true);
            pauseText.SetActive(true);
            //再開の処理はContinueButton.csのOnClick()に書く
        }
    }
}
