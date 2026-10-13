using UnityEngine;
using UnityEngine.InputSystem;

public class ContinueButton : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private GameObject overlay;
    [SerializeField] private GameObject overlay2;
    [SerializeField] private GameObject retryButton;
    [SerializeField] private GameObject returnToMenuButton;
    [SerializeField] private GameObject pauseText;
    [SerializeField] private GameObject pauseManager;

    public void OnClick()
    {
        PauseManager.isPaused = false;
        pauseManager.GetComponent<PauseManager>().Unpause();
    }
}
