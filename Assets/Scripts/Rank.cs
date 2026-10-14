using UnityEngine;
using TMPro;

public class Rank : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankText;
    //上からステージ1~5のスコア境界値を格納する二次元配列
    private int[,] borders =
    {
        { 5000, 4000, 3000, 2000 },
        { 6000, 5000, 4000, 3000 },
        { 7000, 6000, 5000, 4000 },
        { 8000, 7000, 6000, 5000 },
        { 9000, 8000, 7000, 6000 }
    };
    void Start()
    {
        if(ScoreText.score >= borders[StageSelectionButton.currentStageIndex - 1, 0])
        {
            rankText.text = "S";
        }
        else if(ScoreText.score >= borders[StageSelectionButton.currentStageIndex - 1, 1])
        {
            rankText.text = "A";
        }
        else if(ScoreText.score >=  borders[StageSelectionButton.currentStageIndex - 1, 2])
        {
            rankText.text = "B";
        }
        else if(ScoreText.score >= borders[StageSelectionButton.currentStageIndex - 1, 3])
        {
            rankText.text = "C";
        }
        else
        {
            rankText.text = "D";
        }
    }
}
