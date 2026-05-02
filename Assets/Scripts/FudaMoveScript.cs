using UnityEngine;

public class MouseXZMove : MonoBehaviour
{
    public float planeHeight = 2f;

    private bool isDragging = false;
    private bool isDecidedPosition = false;
    private Vector3 offset;

    public float baseExplosionForce = 300f;
    public float chargeRate = 200f;
    private float chargeTime = 0f;
    private bool isCharging = false;

    private bool isFalling = false;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 最初は浮いているので物理無効
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void Update()
    {
        // --- 左クリック押した瞬間 ---
        if (Input.GetMouseButtonDown(0))
        {
            if (!isDecidedPosition)
            {
                TryStartDrag();
            }
            else
            {
                // チャージ開始
                isCharging = true;
                chargeTime = 0f;
            }
        }

        // --- 左クリック離した瞬間 ---
        if (Input.GetMouseButtonUp(0))
        {
            // ドラッグ終了 → 位置確定
            if (isDragging)
            {
                isDragging = false;
                isDecidedPosition = true;
            }

            // チャージ終了 → 落下開始
            if (isCharging)
            {
                isCharging = false;
                StartFalling();
            }
        }

        // --- ドラッグ中 ---
        if (isDragging && !isDecidedPosition)
        {
            DragOnPlane();
        }

        // --- チャージ中 ---
        if (isCharging)
        {
            chargeTime += Time.deltaTime;
        }
    }

    // 落下開始処理（物理ON）
    void StartFalling()
    {
        isFalling = true;

        rb.isKinematic = false; // 物理ON
        rb.useGravity = true;   // 重力ON
    }

    // ドラッグ開始判定
    void TryStartDrag()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == transform)
            {
                Vector3 hitPosOnPlane = GetPointOnPlane(ray);
                offset = transform.position - hitPosOnPlane;
                isDragging = true;
            }
        }
    }

    // ドラッグ中の位置更新
    void DragOnPlane()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Vector3 pointOnPlane = GetPointOnPlane(ray);

        Vector3 targetPos = pointOnPlane + offset;
        targetPos.y = planeHeight;

        transform.position = targetPos;
    }

    // レイと平面の交点
    Vector3 GetPointOnPlane(Ray ray)
    {
        Plane plane = new Plane(Vector3.up, new Vector3(0f, planeHeight, 0f));
        if (plane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }
        return transform.position;
    }

    // 衝突処理：落下中に Bafuda に当たったら爆発
    void OnCollisionEnter(Collision collision)
    {
        if (!isFalling) return;

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