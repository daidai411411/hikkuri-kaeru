using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    //ステージクリア後に上書きされないようcurrentとresultを分けて実装。また誤差を避けるためresultTimeは100倍してintで管理
    public static int resultTime = 0;
    private string text;
    private float currentTime = 0f;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private SceneTransition sceneTransition;
    [SerializeField] private PauseManager pauseManager;
    void Awake()
    {
        currentTime = 0f;
        timerText = GetComponent<TextMeshProUGUI>();
    }
    string FormatNumericPartMonospace(string s)
    {
        string result = "";
        bool inNumber = false;
        foreach (char c in s)
        {
            if (char.IsDigit(c))
            {
                if (!inNumber)
                {
                    result += "<mspace=0.55em>";
                    inNumber = true;
                }
                result += c;
            }
            else
            {
                if (inNumber)
                {
                    result += "</mspace>";
                    inNumber = false;
                }
                result += c;
            }
        }
        if (inNumber)
        {
            result += "</mspace>";
        }
        return result;
    }

    void Update()
    {
        if (!PauseManager.isPaused)
        {
            currentTime += Time.deltaTime;
        }
        int minutes = (int)currentTime / 60;
        int seconds = (int)currentTime % 60;
        int milliseconds = (int)(currentTime % 1 * 100);if (minutes == 0)
        {
            text = $"{seconds:0}.{milliseconds:00}";
        }
        else
        {
            text = $"{minutes:0}:{seconds:00}.{milliseconds:00}";
        }
        timerText.text = FormatNumericPartMonospace(text);
        //テスト用
        if (currentTime >= 4.99f)
        {
            resultTime = (int)(currentTime*100);
            sceneTransition.LoadScene("Result");
        }
    }
}
