using UnityEngine;
using TMPro;

public class Rank : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankText;
    void Start()
    {
        if(ScoreText.score >= 9000)
        {
            rankText.text = "S";
        }
        else if(ScoreText.score >= 8000)
        {
            rankText.text = "A";
        }
        else if(ScoreText.score >= 7000)
        {
            rankText.text = "B";
        }
        else if(ScoreText.score >= 6000)
        {
            rankText.text = "C";
        }
        else
        {
            rankText.text = "D";
        }
    }
}
