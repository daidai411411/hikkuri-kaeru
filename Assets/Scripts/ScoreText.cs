using UnityEngine;

public class ScoreText : MonoBehaviour
{
    public static int score;
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    void Start()
    {
        
        score = 10000 - StageFlipper.flipCount * 100;
        //スコアの計算式ができたら入れる
        scoreText.text = score.ToString();
    }
}
