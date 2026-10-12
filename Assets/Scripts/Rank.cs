using UnityEngine;

public class Rank : MonoBehaviour
{
    void Start()
    {
        if(ScoreText.score >= 9000)
        {
            Debug.Log("S");
        }
        else if(ScoreText.score >= 8000)
        {
            Debug.Log("A");
        }
        else if(ScoreText.score >= 7000)
        {
            Debug.Log("B");
        }
        else if(ScoreText.score >= 6000)
        {
            Debug.Log("C");
        }
        else
        {
            Debug.Log("D");
        }
    }
}
