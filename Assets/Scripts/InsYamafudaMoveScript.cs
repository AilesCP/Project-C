using System.Collections;
using UnityEngine;

public class InsYamafudaMoveScript : MonoBehaviour
{
    public float planeHeight = 2f;

    public bool isDragging = false;
    public bool isDecidedPosition = false;
    private Vector3 offset;

    public float baseExplosionForce = 300f;
    public float chargeRate = 200f;
    private float chargeTime = 0f;
    public bool isCharging = false;

    public bool isFalling = false;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 最初は浮いているので物理無効
        rb.isKinematic = true;
        rb.useGravity = false;

        StartFalling();
    }

    [System.Obsolete]
    void Update()
    {
        
    }

    // 落下開始処理（物理ON）
    [System.Obsolete]
    void StartFalling()
    {
        isFalling = true;

        rb.isKinematic = false;
        rb.useGravity = true;

        Debug.Log("落下開始");

        // タグ変更
        gameObject.tag = "Bafuda";

        // BafudaScript を追加
        var ba = gameObject.AddComponent<BafudaScript>();

        // ★ SuperBattleManager に登録
        SuperBattleManager manager = FindObjectOfType<SuperBattleManager>();
        if (manager != null)
        {
            manager.fieldCards.Add(ba);
        }

        // ★ ターン進行開始
        SuperBattleManager.turnChanging = false;
    }

    // 衝突処理：落下中に Bafuda に当たったら爆発
    void OnCollisionEnter(Collision collision)
    {
        if (!isFalling || SuperBattleManager.isYamafudaDrawing) return;

        if (collision.collider.CompareTag("Bafuda"))
        {
            Rigidbody targetRb = collision.collider.attachedRigidbody;
            if (targetRb != null)
            {
                float power = baseExplosionForce + chargeRate * chargeTime;
                float radius = 3f;
                float upwards = 0.5f;

                targetRb.AddExplosionForce(power, transform.position, radius, upwards, ForceMode.Impulse);
            }

            Debug.Log("Bafuda に衝突 → 爆発させた");
        }
    }
}