using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpPower = 12f;
    private InputAction moveAction;
    private InputAction jumpAction;
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        moveAction = inputActions.FindAction("Stage/Move");
        jumpAction = inputActions.FindAction("Stage/Jump");
        // 開発用シーンにはInputManagerが無いため、ここで有効にする
        moveAction.Enable();
        jumpAction.Enable();
    }

    void Update()
    {
        // Aキーで-1、Dキーで1、何も押していなければ0
        float move = moveAction.ReadValue<float>();
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

        // 地面に立っている時だけジャンプできる
        if (jumpAction.WasPressedThisFrame() && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
        }
    }

    // カエルの足元のすぐ下に、床などの当たり判定があるか調べる
    private bool IsGrounded()
    {
        Bounds bounds = boxCollider.bounds;
        Vector2 checkPosition = new Vector2(bounds.center.x, bounds.min.y - 0.1f);
        Vector2 checkSize = new Vector2(bounds.size.x * 0.9f, 0.1f);
        return Physics2D.OverlapBox(checkPosition, checkSize, 0f) != null;
    }
}
