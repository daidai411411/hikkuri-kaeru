using UnityEngine;

public class ContinueButton : MonoBehaviour
{
    [SerializeField] private GameObject pauseManager;

    public void OnClick()
    {
        pauseManager.GetComponent<PauseManager>().Unpause();
    }
}
