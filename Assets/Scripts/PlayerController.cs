using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private float moveSpeed = 5f;
    private InputAction moveAction;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        moveAction = inputActions.FindAction("Stage/Move");
        // 開発用シーンにはInputManagerが無いため、ここで有効にする
        moveAction.Enable();
    }

    void Update()
    {
        // Aキーで-1、Dキーで1、何も押していなければ0
        float move = moveAction.ReadValue<float>();
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);
    }
}
