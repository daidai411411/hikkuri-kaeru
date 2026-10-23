using UnityEngine;

public class RetryButton : MonoBehaviour
{
    [SerializeField] private SceneTransition sceneTransition;
    public void OnClick()
    {
        sceneTransition.LoadScene("Stage" + StageSelectionButton.currentStageIndex);
    }
}
