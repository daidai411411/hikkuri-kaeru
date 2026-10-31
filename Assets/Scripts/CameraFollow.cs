using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float flipDuration = 0.4f;
    private float flipTimer = 0f;

    // 反転演出中かどうか
    public bool IsFlipping()
    {
        return flipTimer > 0f;
    }

    // 反転した瞬間に呼ぶ（画面を180度傾けた状態から、元の向きへ戻していく）
    public void StartFlipEffect()
    {
        flipTimer = flipDuration;
    }

    // カエルが動いた後に、カメラをカエルの位置に合わせる
    void LateUpdate()
    {
        transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);

        if (flipTimer > 0f)
        {
            flipTimer -= Time.deltaTime;
            if (flipTimer < 0f)
            {
                flipTimer = 0f;
            }
            // 残り時間に合わせて、180度から0度へなめらかに回す
            float angle = Mathf.SmoothStep(0f, 180f, flipTimer / flipDuration);
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
