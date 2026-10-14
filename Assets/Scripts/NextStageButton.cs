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
        StageSelectionButton.currentStageIndex++;
        sceneTransition.LoadScene("Stage" + StageSelectionButton.currentStageIndex);
    }
}
