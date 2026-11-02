using UnityEngine;
using TMPro;

public class Rank : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankText;
    //上からステージ1~5のスコア境界値を格納する二次元配列
    //S は最低反転回数・最速タイム（2026-11-02 の通しプレイ）の点から10秒ぶん（1000点）引いて100点単位で切り下げた値。A〜C はそこから1000点ずつ
    private int[,] borders =
    {
        { 8000, 7000, 6000, 5000 },
        { 6900, 5900, 4900, 3900 },
        { 7000, 6000, 5000, 4000 },
        { 6900, 5900, 4900, 3900 },
        { 6700, 5700, 4700, 3700 }
    };
    void Start()
    {   if(StageSelectionButton.currentStageIndex == 0)
        {
            Debug.LogError("ステージのインデックスが0です");
            return;
        }
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
