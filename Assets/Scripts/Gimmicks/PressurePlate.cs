using UnityEngine;
using UnityEngine.Events;

// 上面にカエルか岩が乗ると onPressed を呼ぶ台座。つなぐ先（つるの門など）はインスペクタで設定する
// 上面が下を向いている間（天井にある間）は押せない。ジャンプで下から頭突きしても押せないようにするため
public class PressurePlate : MonoBehaviour
{
    [Tooltip("押される面のコライダー（子の Top に付いているもの）。\nこの上面のすぐ上に乗ったものを判定する")]
    [SerializeField] private BoxCollider2D pressSurface;
    [Tooltip("オフ：一度乗ったら押されたままになる。\nオン：降りたら On Released を呼ぶ（乗っている間だけ門が開く）")]
    [SerializeField] private bool releaseWhenLeft = false;
    [Tooltip("上面からどれだけの高さまでを「乗っている」とみなすか")]
    [SerializeField] private float detectHeight = 0.1f;
    [Tooltip("乗ったときに呼ぶ処理。つるの門の Open をつなぐ")]
    [SerializeField] private UnityEvent onPressed;
    [Tooltip("降りたときに呼ぶ処理（Release When Left がオンのときだけ）。つるの門の Close をつなぐ")]
    [SerializeField] private UnityEvent onReleased;

    private bool isPressed;

    void FixedUpdate()
    {
        bool isSomethingOnTop = IsFacingUp() && IsSomethingOnTop();

        if (isSomethingOnTop && !isPressed)
        {
            isPressed = true;
            onPressed.Invoke();
        }
        else if (!isSomethingOnTop && isPressed && releaseWhenLeft)
        {
            isPressed = false;
            onReleased.Invoke();
        }
    }

    private bool IsFacingUp()
    {
        return Vector2.Dot(transform.up, Vector2.up) > 0f;
    }

    // 上面のすぐ上の薄い範囲に、動く物体（カエル・岩）がいるか調べる
    private bool IsSomethingOnTop()
    {
        Transform surfaceTransform = pressSurface.transform;
        Vector2 surfaceSize = pressSurface.size;
        Vector2 checkPosition = (Vector2)surfaceTransform.TransformPoint(pressSurface.offset)
            + (Vector2)transform.up * (surfaceSize.y / 2f + detectHeight / 2f);
        Vector2 checkSize = new Vector2(surfaceSize.x, detectHeight);

        foreach (Collider2D hit in Physics2D.OverlapBoxAll(checkPosition, checkSize, transform.eulerAngles.z))
        {
            Rigidbody2D hitBody = hit.attachedRigidbody;
            if (hitBody != null && hitBody.bodyType == RigidbodyType2D.Dynamic)
            {
                return true;
            }
        }
        return false;
    }
}
