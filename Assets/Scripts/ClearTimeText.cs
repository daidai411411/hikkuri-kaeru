using UnityEngine;

public class ClearTimeText : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI clearTimeText;
    void Start()
    {
        //時間を実装し次第ここも実装
        clearTimeText.text = "00:00";
    }

}
