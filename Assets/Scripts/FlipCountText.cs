using UnityEngine;

public class FlipCountText : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI flipCountText;
    void Start()
    {
        flipCountText.text = StageFlipper.flipCount + "回";
    }
}
