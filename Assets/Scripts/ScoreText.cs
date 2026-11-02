using UnityEngine;

public class ScoreText : MonoBehaviour
{
    public static int score;
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;

    // ステージ1〜5の最低反転回数（2026-11-02 の通しプレイで実測）。
    // ステージごとに必要な反転の数が違うので、最低回数を超えた分だけ減点する。先頭の0は直接起動（ステージ番号0）用
    private static readonly int[] minFlipCounts = { 0, 4, 2, 3, 5, 11 };

    void Awake()
    {
        //1万-（反転回数-最低反転回数）*1000-クリアタイム(秒)*100
        int extraFlips = Mathf.Max(0, StageFlipper.flipCount - MinFlipCount(StageSelectionButton.currentStageIndex));
        score = 10000 - extraFlips * 1000 - Timer.resultTime;
        scoreText.text = score.ToString();
        PlayTestLogger.OutputLog(
            "Stage" + StageSelectionButton.currentStageIndex.ToString(),
            Timer.resultTime,
            StageFlipper.flipCount,
            score
        );
    }

    private static int MinFlipCount(int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= minFlipCounts.Length)
        {
            return 0;
        }
        return minFlipCounts[stageIndex];
    }
}
