using UnityEngine;

public class NextStageButton : MonoBehaviour
{
    [SerializeField] private SceneTransition sceneTransition;
    void Start()
    {
        if (StageSelectionButton.currentStageIndex > 5)
        {
            gameObject.SetActive(false);
        }
    }
    public void OnClick()
    {
        sceneTransition.LoadScene("Stage" + (StageSelectionButton.currentStageIndex + 1));
    }
}
