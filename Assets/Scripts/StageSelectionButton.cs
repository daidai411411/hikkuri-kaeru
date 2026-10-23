using UnityEngine;

public class StageSelectionButton : MonoBehaviour
{
    [SerializeField] private int stageIndex;
    public static int currentStageIndex;
    [SerializeField] private SceneTransition sceneTransition;

    public void OnClick()
    {
        currentStageIndex = stageIndex;
        sceneTransition.LoadScene("Stage" + stageIndex);
    }

}
