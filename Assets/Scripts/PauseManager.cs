using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

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
    static public bool isPaused = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        pauseAction = inputActions.FindAction("Stage/Pause");
    }
    
    public void Pause()
    {            
            Time.timeScale = 0f;
            overlay.SetActive(true);
            overlay2.SetActive(true);
            retryButton.SetActive(true);
            returnToMenuButton.SetActive(true);
            continueButton.SetActive(true);
            pauseText.SetActive(true);
    }

    public void Unpause()
    {
        Time.timeScale = 1f;
        overlay.SetActive(false);
        overlay2.SetActive(false);
        retryButton.SetActive(false);
        returnToMenuButton.SetActive(false);
        continueButton.SetActive(false);
        pauseText.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }


    void Update()
    {
        if(pauseAction.WasPressedThisFrame())
        {
            isPaused = !isPaused;
            if(isPaused)
            {
                Pause();
            }
            else
            {
                Unpause();
            }
        }
    }
}
