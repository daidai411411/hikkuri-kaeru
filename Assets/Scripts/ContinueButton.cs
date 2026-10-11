using UnityEngine;
using UnityEngine.InputSystem;

public class ContinueButton : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private GameObject overlay;
    [SerializeField] private GameObject retryButton;
    [SerializeField] private GameObject returnToMenuButton;
    private InputAction PauseAction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        PauseAction = inputActions.FindAction("Stage/Pause");
    }

    void Pause()
    {
        overlay.SetActive(false);
        retryButton.SetActive(false);
        returnToMenuButton.SetActive(false);
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
    public void OnClick()
    {
        Pause();
    }
    void Update()
    {
        if(PauseAction.WasPressedThisFrame())
        {
            Pause();
        }
    }
}
