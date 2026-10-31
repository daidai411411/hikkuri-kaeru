using UnityEngine;

public class ClearTimeText : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI clearTimeText;
    void Awake()
    {
        clearTimeText.text = (Timer.resultTime/100f).ToString("0.00") + "秒";
    }

}
