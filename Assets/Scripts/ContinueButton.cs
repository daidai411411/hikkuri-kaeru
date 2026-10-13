using UnityEngine;

public class ContinueButton : MonoBehaviour
{
    [SerializeField] private GameObject pauseManager;

    public void OnClick()
    {
        PauseManager.isPaused = false;
        pauseManager.GetComponent<PauseManager>().Unpause();
    }
}
