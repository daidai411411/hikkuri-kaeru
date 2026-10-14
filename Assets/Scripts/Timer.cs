using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public static float currentTime = 0f;
    [SerializeField] private TextMeshProUGUI timerText;
    void Awake()
    {
        currentTime = 0f;
        timerText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (PauseManager.isPaused)
        {
            currentTime += Time.deltaTime;
        }
        int minutes = (int)currentTime / 60;
        int seconds = (int)currentTime % 60;
        int milliseconds = (int)(currentTime % 1 * 100);
        if(seconds == 0 && minutes == 0)
        {
            timerText.text = $"{milliseconds:0}";
        }
        else if (minutes == 0)
        {
            timerText.text = $"{seconds:0}.{milliseconds:00}";
        }
        else
        {
            timerText.text = $"{minutes:0}:{seconds:00}.{milliseconds:00}";
        }
    }
}
