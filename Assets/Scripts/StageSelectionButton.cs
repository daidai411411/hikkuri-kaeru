using UnityEngine;

public class StageSelectionButton : MonoBehaviour
{
    [SerializeField] private int stageIndex;
    public static int currentStageIndex;
    public SceneTransition sceneTransition;

    public void OnClick()
    {
        currentStageIndex = stageIndex;
        sceneTransition.LoadScene("Stage");
    }

}
