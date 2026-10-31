using UnityEngine;

public class ScoreText : MonoBehaviour
{
    public static int score;
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    void Awake()
    {
        //仮計算式、1万-反転回数*1000-クリアタイム(秒)*100
        score = 10000 - StageFlipper.flipCount * 1000 - Timer.resultTime;
        scoreText.text = score.ToString();
        PlayTestLogger.OutputLog(
            "Stage" + StageSelectionButton.currentStageIndex.ToString(),
            Timer.resultTime,
            StageFlipper.flipCount,
            score
        );
    }
}
