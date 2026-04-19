using UnityEngine;

public class ClickSpawnExplosion : MonoBehaviour
{
    public Camera cam;              // 使用するカメラ（未設定なら MainCamera）
    public GameObject prefab;       // クリック地点に生成するプレハブ
    public float explosionForce = 500f;   // 吹き飛ばす力
    public float explosionRadius = 5f;    // 爆発半径

    void Start()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 spawnPos = hit.point;

                // ① プレハブをクリック地点に生成
                GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);

                // ② 周囲の Rigidbody を吹き飛ばす
                Collider[] cols = Physics.OverlapSphere(spawnPos, explosionRadius);

                foreach (var col in cols)
                {
                    Rigidbody rb = col.attachedRigidbody;
                    if (rb != null)
                    {
                        rb.AddExplosionForce(
                            explosionForce,
                            spawnPos,
                            explosionRadius,
                            0f,
                            ForceMode.Impulse
                        );
                    }
                }
            }
        }
    }
}