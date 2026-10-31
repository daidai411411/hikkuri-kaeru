using UnityEngine;
using TMPro;

public class StageName : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stageNameText;
    void Start()
    {
        stageNameText.text = "Stage " + StageSelectionButton.currentStageIndex;
    }
}
