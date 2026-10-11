using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class ContinueButton : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private GameObject overlay;
    [SerializeField] private GameObject overlay2;
    [SerializeField] private GameObject retryButton;
    [SerializeField] private GameObject returnToMenuButton;
    [SerializeField] private GameObject pauseText;
    private InputAction PauseAction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        PauseAction = inputActions.FindAction("Stage/Pause");
    }

    void Pause()
    {
        overlay.SetActive(false);
        overlay2.SetActive(false);
        retryButton.SetActive(false);
        returnToMenuButton.SetActive(false);
        pauseText.SetActive(false);
        gameObject.SetActive(false);
        Time.timeScale = 1f;
        EventSystem.current.SetSelectedGameObject(null);
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
