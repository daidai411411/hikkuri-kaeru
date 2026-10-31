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
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        moveAction = inputActions.FindAction("Stage/Move");
        jumpAction = inputActions.FindAction("Stage/Jump");
        // 開発用シーンにはInputManagerが無いため、ここで有効にする
        moveAction.Enable();
        jumpAction.Enable();
    }

    void Update()
    {
        bool isGrounded = IsGrounded();

        // Aキーで-1、Dキーで1、何も押していなければ0
        float move = moveAction.ReadValue<float>();
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

        // 地面に立っている時だけジャンプできる
        if (jumpAction.WasPressedThisFrame() && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
        }

        // 進む方向に合わせて左右の向きを変える（画像は右向きの想定）
        if (move != 0)
        {
            spriteRenderer.flipX = move < 0;
        }

        // アニメーションの切り替えに使う値をAnimatorに渡す
        animator.SetFloat("Speed", Mathf.Abs(move));
        animator.SetBool("IsGrounded", isGrounded);
    }

    // カエルの足元のすぐ下に、床などの当たり判定があるか調べる
    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));
        return hit.collider != null;
    }

    private void OnDrawGizmos()
    {
        if (boxCollider == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boxCollider.bounds.center + Vector3.down * 0.1f, boxCollider.bounds.size);
    }
}
