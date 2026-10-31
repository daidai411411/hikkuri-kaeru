using UnityEngine;
using UnityEngine.InputSystem;

public class StageFlipper : MonoBehaviour
{
    // 反転回数（リザルト画面などから StageFlipper.flipCount で読める）
    public static int flipCount;

    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private Transform stageRoot;
    private InputAction invertAction;
    private CameraFollow cameraFollow;

    void Awake()
    {
        flipCount = 0;
        invertAction = inputActions.FindAction("Stage/Invert");
        // 開発用シーンにはInputManagerが無いため、ここで有効にする
        invertAction.Enable();
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
    }

    void Update()
    {
        // ポーズ中と反転演出中は反転できない
        if (invertAction.WasPressedThisFrame() && !PauseManager.isPaused && !cameraFollow.IsFlipping())
        {
            // カエルの位置を中心に、ステージ全体を180度回す
            stageRoot.RotateAround(transform.position, Vector3.forward, 180f);
            flipCount++;
            cameraFollow.StartFlipEffect();
        }
    }
}
