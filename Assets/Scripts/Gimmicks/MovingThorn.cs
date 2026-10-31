using UnityEngine;

// 一定の間隔で「隠れる → 出てくる場所を予告 → 床からせり上がる → 出たまま → 沈む」を繰り返すいばら
// 当たり判定は本体と一緒に動くので、床から出ている部分に触れたときだけ死ぬ
// 沈んでいる間は当たり判定を切る。着地の瞬間にカエルが床へ少しめり込み、床のすぐ下のいばらに触れて死ぬのを防ぐため
public class MovingThorn : MonoBehaviour
{
    [Header("参照（プレハブで設定済み。触らなくてよい）")]
    [Tooltip("上下に動くいばら本体。\n置いた位置が「出ている位置」になる")]
    [SerializeField] private Transform body;
    [Tooltip("予告中だけ表示する、床の表面の印")]
    [SerializeField] private GameObject warning;

    [Header("沈める深さ")]
    [Tooltip("Body が出ている位置から床の中へ沈める距離。\nBody の高さより少し大きくして、沈んだときに床の表面から出ないようにする。\n床の厚さはこれより大きくすること")]
    [SerializeField] private float sinkDistance = 0.55f;

    [Header("タイミング（秒）")]
    [Tooltip("床の中に隠れている時間。\n予告は出ず、触れても安全")]
    [SerializeField] private float hiddenDuration = 2f;
    [Tooltip("出てくる場所に印を出して予告する時間。\nまだ安全")]
    [SerializeField] private float warningDuration = 1f;
    [Tooltip("床からせり上がるのにかかる時間。\n床から出た部分に触れると死ぬ")]
    [SerializeField] private float riseDuration = 0.15f;
    [Tooltip("出きったまま止まっている時間。\n触れると死ぬ")]
    [SerializeField] private float outDuration = 1.5f;
    [Tooltip("床の中へ沈むのにかかる時間。\n沈みきるまで、床から出ている部分に触れると死ぬ")]
    [SerializeField] private float sinkDuration = 0.3f;
    [Tooltip("周期の途中から始める秒数。\n並べたいばらを順番に出したいときに、1本ずつずらして設定する")]
    [SerializeField] private float startOffset = 0f;

    private Collider2D bodyCollider;
    private Vector3 outPosition;
    private Vector3 sunkPosition;
    private float elapsed;

    void Awake()
    {
        bodyCollider = body.GetComponent<Collider2D>();
        outPosition = body.localPosition;
        sunkPosition = outPosition + Vector3.down * sinkDistance;
        elapsed = startOffset;
    }

    void Update()
    {
        float warningEnd = hiddenDuration + warningDuration;
        float riseEnd = warningEnd + riseDuration;
        float outEnd = riseEnd + outDuration;
        float cycleDuration = outEnd + sinkDuration;
        elapsed = (elapsed + Time.deltaTime) % cycleDuration;

        warning.SetActive(elapsed >= hiddenDuration && elapsed < warningEnd);
        bodyCollider.enabled = elapsed >= warningEnd;

        if (elapsed < warningEnd)
        {
            body.localPosition = sunkPosition;
        }
        else if (elapsed < riseEnd)
        {
            body.localPosition = Vector3.Lerp(sunkPosition, outPosition, (elapsed - warningEnd) / riseDuration);
        }
        else if (elapsed < outEnd)
        {
            body.localPosition = outPosition;
        }
        else
        {
            body.localPosition = Vector3.Lerp(outPosition, sunkPosition, (elapsed - outEnd) / sinkDuration);
        }
    }
}
