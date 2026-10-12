using UnityEngine;

public class ScoreText : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    void Start()
    {
        //スコアの計算式ができたら入れる
        scoreText.text = (10000 - StageFlipper.flipCount * 100).ToString();
    }
}
