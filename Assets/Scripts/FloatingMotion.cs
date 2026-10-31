using UnityEngine;

/// <summary>
/// UI 要素を初期位置を中心に sin 波で上下へ往復させる。
/// 開始時の anchoredPosition を中心とするため、レイアウト確定後の位置から動き出す。
/// Time.timeScale の影響を受けない。
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class FloatingMotion : MonoBehaviour
{
    [SerializeField] private float amplitude = 10f;
    [SerializeField, Min(0.01f)] private float period = 2f;
    private RectTransform rectTransform;
    private Vector2 basePosition;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        basePosition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        float offset = Mathf.Sin(2f * Mathf.PI * Time.unscaledTime / period) * amplitude;
        rectTransform.anchoredPosition = basePosition + new Vector2(0f, offset);
    }
}
