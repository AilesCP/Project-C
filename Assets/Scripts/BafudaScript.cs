using UnityEngine;

public class BafudaScript : MonoBehaviour
{
    public Rigidbody rb;
    public bool isStopped = false;
    float stillTime = 0f;

    // ★ 一度だけ無効化するためのフラグ
    private bool disabledMoveScript = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        isStopped = false;
    }

    void Update()
    {
        // --- 停止判定 ---
        if (rb.linearVelocity.magnitude < 0.05f)
        {
            stillTime += Time.deltaTime;

            if (stillTime > 0.5f)
            {
                if (!isStopped)
                {
                    isStopped = true;
                    OnStopped();   // ★ 止まった瞬間だけ呼ぶ
                }
            }
        }
        else
        {
            stillTime = 0f;
            isStopped = false;
        }
    }

    // ★ 止まった瞬間だけ呼ばれる
    void OnStopped()
    {
        if (disabledMoveScript) return; // 1回だけ実行

        // ▼ ここで落とす札のスクリプトを無効化する
        var move = GetComponent<InsTefudaMoveScript>();  // ← ここを InsTefudaMoveScript に変えてもOK
        if (move != null)
        {
            move.enabled = false;
        }

        disabledMoveScript = true;
    }
}