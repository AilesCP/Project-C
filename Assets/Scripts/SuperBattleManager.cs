using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperBattleManager : MonoBehaviour
{
    public Camera cam;
    public GameObject yamafudaPrefab;
    public GameObject myTefudaPrefab;
    public GameObject yourTefudaPrefab;
    public GameObject bafudaPrefab;
    public GameObject cardPrefab;
    public GameObject insTefudaPrefab;

    public float explosionForce = 500f;
    public float explosionRadius = 5f;

    public Transform parentMyTefudaTransform;
    public Transform parentYourTefudaTransform;
    public Transform parentYamafudaTransform;
    public Transform parentBafudaTransform;
    public Transform parentHikariTransform;
    public Transform parentTaneTransform;
    public Transform parentTanzakuTransform;
    public Transform parentKasuTransform;
    public Transform parentDrowTefuda;
    public Transform parentInsTefuda;

    private int yamafudaCount = 8;
    private int tefudaCount = 8;

    private int selectCardNum = 0;
    private int drowYamafudaNum = 0;
    private int currentYamafudaIndex = 0;
    private GameObject selectObj = null;
    private GameObject selectYamafudaObj = null;
    private bool isSelectTefuda = false;
    private int handYamafudaIndex = 0;

    public Material[] cardMaterials;
    List<int> numbers = new List<int>();
    List<int> emptyBafudaSlots = new List<int>();
    GameObject[] bafudaSlots = new GameObject[8];
    Queue<int> q;

    private bool isTurn = false;
    private bool hasCheckedThisTurn = false;
    private bool isYamafudaDrawing = false;
    private bool isYamafudaMatching = false;

    // ★ 元からあった cardType（役札の種類）
    private int[] cardType = {
        1, 3, 4, 4, 2, 3, 4, 4, 1, 3, 4, 4,
        2, 3, 4, 4, 2, 3, 4, 4, 2, 3, 4, 4,
        2, 3, 4, 4, 1, 2, 4, 4, 2, 3, 4, 4,
        2, 3, 4, 4, 1, 2, 3, 4, 1, 4, 4, 4
    };

    // ★ 追加された cardMonth（1〜12 の月）
    private int[] cardMonth = {
        1,1,1,1, 2,2,2,2, 3,3,3,3, 4,4,4,4,
        5,5,5,5, 6,6,6,6, 7,7,7,7, 8,8,8,8,
        9,9,9,9, 10,10,10,10, 11,11,11,11, 12,12,12,12
    };

    private int[] cardTypeCount = { 0, 0, 0, 0 };

    void Start()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }
        instanceYamafudaCards();
        StartCoroutine(devideTefudaCards());
    }

    void Update()
    {
        if (isTurn && !hasCheckedThisTurn)
        {
            StartTurn();
            hasCheckedThisTurn = true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject obj = hit.collider.gameObject;

                // ============================
                // ★ 手札クリック（CardInfo 方式）
                // ============================
                if (obj.CompareTag("MyTefuda"))
                {
                    if (!isTurn || isYamafudaDrawing) return;

                    CardInfo info = obj.GetComponent<CardInfo>();
                    if (info == null) return;

                    int handIndex = info.cardIndex;
                    selectCardNum = handIndex;

                    // 空中に手札を生成
                    insSelectTefuda();
                    Destroy(obj);
                    
                    // ★ 一致する月が存在しない場合 → 場札に追加
                    /*if (!IsAnyMonthMatch(parentBafudaTransform, parentMyTefudaTransform))
                    {
                        Debug.Log("一致する月が無いので、手札を場札に追加します");

                        AddCardToBafuda(handIndex);

                        Destroy(obj); // 手札を消す

                        // 山札から次の札を引く
                        drowNextYamafuda();
                        return;
                    }

                    // ★ 一致がある場合 → いつも通り場札クリック待ち
                    Debug.Log("一致する月があります。");
                    selectCardNum = handIndex;
                    isSelectTefuda = true;
                    selectObj = obj;*/

                    return;
                }

                // ============================
                // ★ 爆発用
                // ============================
                if (obj.CompareTag("Mosen"))
                {
                    Vector3 spawnPos = hit.point;

                    GameObject expObj = Instantiate(cardPrefab, spawnPos, Quaternion.identity);

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
                    return;
                }

                // ============================
                // ★ 場札クリック（CardInfo 方式）
                // ============================
                if (obj.CompareTag("Bafuda"))
                {
                    if (!isYamafudaMatching && !isSelectTefuda) return;

                    CardInfo info = obj.GetComponent<CardInfo>();
                    if (info == null) return;

                    int baIndex = info.cardIndex;

                    Debug.Log("クリックした場札 index = " + baIndex +
                              " / 月 = " + cardMonth[baIndex] +
                              " / タイプ = " + cardType[baIndex]);

                    if (isYamafudaMatching)
                    {
                        CardInfo yamafudaInfo = selectYamafudaObj.GetComponent<CardInfo>();
                        if (yamafudaInfo == null) return;

                        handYamafudaIndex = yamafudaInfo.cardIndex;
                        selectCardNum = handYamafudaIndex;
                    }

                    // ★ 月一致判定（Material 比較なし）
                    if (cardMonth[selectCardNum] == cardMonth[baIndex])
                    {
                        Debug.Log("一致");
                        // ============================
                        // ★ 一致したときの処理
                        // ============================

                        GameObject objField = Instantiate(
                            yamafudaPrefab,
                            new Vector3(
                                obj.transform.position.x - 0.05f,
                                obj.transform.position.y + 0.01f,
                                obj.transform.position.z - 0.05f
                            ),
                            Quaternion.Euler(0, -90, 0)
                        );

                        GameObject obj2 = null;

                        // ★ 場札 → 持ち札
                        if (cardType[baIndex] == 1)
                        {
                            obj2 = Instantiate(yamafudaPrefab, parentHikariTransform);
                            obj2.transform.localPosition = new Vector3(0.55f, 0.465f + (cardTypeCount[0] * 0.01f), -2.35f - (cardTypeCount[0] * 0.1f));
                            obj2.transform.localRotation = Quaternion.Euler(0, -90, 0);
                            cardTypeCount[0]++;
                        }
                        else if (cardType[baIndex] == 2)
                        {
                            obj2 = Instantiate(yamafudaPrefab, parentTaneTransform);
                            obj2.transform.localPosition = new Vector3(-0.15f, 0.465f + (cardTypeCount[1] * 0.01f), -2.35f - (cardTypeCount[1] * 0.1f));
                            obj2.transform.localRotation = Quaternion.Euler(0, -90, 0);
                            cardTypeCount[1]++;
                        }
                        else if (cardType[baIndex] == 3)
                        {
                            obj2 = Instantiate(yamafudaPrefab, parentTanzakuTransform);
                            obj2.transform.localPosition = new Vector3(-0.85f, 0.465f + (cardTypeCount[2] * 0.01f), -2.35f - (cardTypeCount[2] * 0.1f));
                            obj2.transform.localRotation = Quaternion.Euler(0, -90, 0);
                            cardTypeCount[2]++;
                        }
                        else if (cardType[baIndex] == 4)
                        {
                            obj2 = Instantiate(yamafudaPrefab, parentKasuTransform);
                            obj2.transform.localPosition = new Vector3(-1.55f, 0.465f + (cardTypeCount[3] * 0.01f), -2.35f - (cardTypeCount[3] * 0.1f));
                            obj2.transform.localRotation = Quaternion.Euler(0, -90, 0);
                            cardTypeCount[3]++;
                        }

                        GameObject obj3 = null;

                        // ★ 手札 → 持ち札
                        if (cardType[selectCardNum] == 1)
                        {
                            obj3 = Instantiate(yamafudaPrefab, parentHikariTransform);
                            obj3.transform.localPosition = new Vector3(0.55f, 0.465f + (cardTypeCount[0] * 0.01f), -2.35f - (cardTypeCount[0] * 0.1f));
                            obj3.transform.localRotation = Quaternion.Euler(0, -90, 0);
                            cardTypeCount[0]++;
                        }
                        else if (cardType[selectCardNum] == 2)
                        {
                            obj3 = Instantiate(yamafudaPrefab, parentTaneTransform);
                            obj3.transform.localPosition = new Vector3(-0.15f, 0.465f + (cardTypeCount[1] * 0.01f), -2.35f - (cardTypeCount[1] * 0.1f));
                            obj3.transform.localRotation = Quaternion.Euler(0, -90, 0);
                            cardTypeCount[1]++;
                        }
                        else if (cardType[selectCardNum] == 3)
                        {
                            obj3 = Instantiate(yamafudaPrefab, parentTanzakuTransform);
                            obj3.transform.localPosition = new Vector3(-0.85f, 0.465f + (cardTypeCount[2] * 0.01f), -2.35f - (cardTypeCount[2] * 0.1f));
                            obj3.transform.localRotation = Quaternion.Euler(0, -90, 0);
                            cardTypeCount[2]++;
                        }
                        else if (cardType[selectCardNum] == 4)
                        {
                            obj3 = Instantiate(yamafudaPrefab, parentKasuTransform);
                            obj3.transform.localPosition = new Vector3(-1.55f, 0.465f + (cardTypeCount[3] * 0.01f), -2.35f - (cardTypeCount[3] * 0.1f));
                            obj3.transform.localRotation = Quaternion.Euler(0, -90, 0);
                            cardTypeCount[3]++;
                        }

                        // ★ Material 設定
                        objField.transform.Find("Flont").GetComponent<Renderer>().material = cardMaterials[selectCardNum];
                        obj2.transform.Find("Flont").GetComponent<Renderer>().material = cardMaterials[baIndex];
                        obj3.transform.Find("Flont").GetComponent<Renderer>().material = cardMaterials[selectCardNum];

                        if (isYamafudaMatching)
                        {
                            int slotIndex = System.Array.IndexOf(bafudaSlots, obj);
                            bafudaSlots[slotIndex] = null;
                            Destroy(objField);
                            Destroy(selectYamafudaObj);
                            Destroy(obj);
                            isYamafudaMatching = false;
                        }
                        else
                        {
                            int slotIndex = System.Array.IndexOf(bafudaSlots, obj);
                            bafudaSlots[slotIndex] = null;
                            Destroy(selectObj);
                            Destroy(objField);
                            Destroy(obj);
                        }

                        isSelectTefuda = false;

                        if (!isYamafudaDrawing)
                        {
                            // 山札から次の札を引く
                            drowNextYamafuda();
                            if (!IsAnyMonthMatch(parentBafudaTransform, parentDrowTefuda))
                            {
                                Debug.Log("一致する札がありません");
                                CardInfo yamafudaInfo = selectYamafudaObj.GetComponent<CardInfo>();
                                if (yamafudaInfo == null) return;

                                handYamafudaIndex = yamafudaInfo.cardIndex;
                                AddCardToBafuda(handYamafudaIndex);

                                Destroy(selectYamafudaObj); // 手札を消す
                                isTurn = false;
                                return;
                            }
                            else
                            {
                                isYamafudaDrawing = true;
                                isYamafudaMatching = true;
                                Debug.Log("一致する札があります。");
                                return;
                            }
                        }
                        isTurn = false;
                        isYamafudaDrawing = false;
                        return;
                    }
                }
            }
        }
    }

    void instanceYamafudaCards()
    {
        for (int i = 0; i < yamafudaCount; i++)
        {
            GameObject obj = Instantiate(yamafudaPrefab, parentYamafudaTransform);
            obj.transform.localPosition = new Vector3(0, 0.465f + (i * 0.01f), 1.7f);
            obj.transform.localRotation = Quaternion.Euler(180, -90, 0);
        }
    }

    IEnumerator devideTefudaCards()
    {
        for (int i = 0; i < cardMaterials.Length; i++)
        {
            numbers.Add(i);
        }

        for (int i = numbers.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            (numbers[i], numbers[r]) = (numbers[r], numbers[i]);
        }

        q = new Queue<int>(numbers);

        for (int i = 0; i < tefudaCount; i++)
        {
            // 自分
            GameObject myObj = Instantiate(myTefudaPrefab, parentMyTefudaTransform);
            myObj.transform.localPosition = new Vector3(-1.5f, 0.465f, -1.4f + (i * 0.5f));
            myObj.transform.localRotation = Quaternion.Euler(0, -90, 0);

            // ★ ここで index を決める
            int idxMy = GetNextYamafuda();
            myObj.GetComponent<CardInfo>().cardIndex = idxMy;

            // Material を貼る
            Transform flont = myObj.transform.Find("Flont");
            Renderer rend = flont.GetComponent<Renderer>();
            rend.material = cardMaterials[idxMy];

            // 相手
            GameObject yourObj = Instantiate(yourTefudaPrefab, parentYourTefudaTransform);
            yourObj.transform.localPosition = new Vector3(1.5f, 0.465f, 1.4f - (i * 0.5f));
            yourObj.transform.localRotation = Quaternion.Euler(180, 90, 0);

            int idxYour = GetNextYamafuda();
            yourObj.GetComponent<CardInfo>().cardIndex = idxYour;

            Transform flont2 = yourObj.transform.Find("Flont");
            Renderer rend2 = flont2.GetComponent<Renderer>();
            rend2.material = cardMaterials[idxYour];

            // 場札
            GameObject BaObj = Instantiate(bafudaPrefab, parentBafudaTransform);

            // スロット位置に配置
            BaObj.transform.localPosition = GetBafudaSlotPosition(i);
            BaObj.transform.localRotation = Quaternion.Euler(0, -90, 0);

            // index をセット
            int idxBa = GetNextYamafuda();
            BaObj.GetComponent<CardInfo>().cardIndex = idxBa;

            // Material
            BaObj.transform.Find("Flont").GetComponent<Renderer>().material = cardMaterials[idxBa];

            // ★ スロットに登録
            bafudaSlots[i] = BaObj;

            yield return new WaitForSeconds(0.2f);
        }
        isTurn = true;
    }

    // 手札と場札の「月」が一致するカードの index を全部返す
    List<int> GetMatchCardIndexes()
    {
        HashSet<int> baMonths = new HashSet<int>();

        // 場札の月を集める
        foreach (Transform child in parentBafudaTransform)
        {
            CardInfo info = child.GetComponent<CardInfo>();
            if (info == null) continue;

            baMonths.Add(cardMonth[info.cardIndex]);
        }

        // 手札の中で一致するカードを集める
        List<int> matchIndexes = new List<int>();

        foreach (Transform child in parentMyTefudaTransform)
        {
            CardInfo info = child.GetComponent<CardInfo>();
            if (info == null) continue;

            if (baMonths.Contains(cardMonth[info.cardIndex]))
            {
                matchIndexes.Add(info.cardIndex);
            }
        }

        return matchIndexes;
    }
    bool IsAnyMonthMatch(Transform parent1, Transform parent2)
    {
        HashSet<int> baMonths = new HashSet<int>();

        // 場札の月を集める
        foreach (Transform child in parent1)
        {
            CardInfo info = child.GetComponent<CardInfo>();
            if (info == null) continue;

            baMonths.Add(cardMonth[info.cardIndex]);
        }

        // 手札の中に一致する月があるかチェック
        foreach (Transform child in parent2)
        {
            CardInfo info = child.GetComponent<CardInfo>();
            if (info == null) continue;

            if (baMonths.Contains(cardMonth[info.cardIndex]))
            {
                return true; // ← 一致が1つでもあれば即true
            }
        }

        return false; // ← 1つも一致しなければfalse
    }

    void StartTurn()
    {
        Debug.Log("---- 場札の状態 ----");
        foreach (Transform child in parentBafudaTransform)
        {
            CardInfo info = child.GetComponent<CardInfo>();
            if (info == null)
            {
                Debug.Log("Bafuda child に CardInfo が付いてない");
                continue;
            }

            int idx = info.cardIndex;
            Debug.Log("場札 index = " + idx + " / 月 = " + cardMonth[idx] + " / タイプ = " + cardType[idx]);
        }
        Debug.Log("---- ここまで ----");

        List<int> matches = GetMatchCardIndexes();

        if (matches.Count == 0)
        {
            Debug.Log("一致する手札（月）はありません");
        }
        else
        {
            Debug.Log("一致する手札のカード：");
            foreach (int idx in matches)
            {
                Debug.Log("手札 index = " + idx + " / 月 = " + cardMonth[idx] + " / タイプ = " + cardType[idx]);
            }
        }
    }

    int GetNextYamafuda()
    {
        return q.Dequeue();
    }
    int GetFirstEmptyBafudaSlot()
    {
        // 空きスロットがあれば最優先で使う
        if (emptyBafudaSlots.Count > 0)
        {
            emptyBafudaSlots.Sort();
            int slot = emptyBafudaSlots[0];
            emptyBafudaSlots.RemoveAt(0);
            return slot;
        }

        // 空きが無ければ末尾
        return parentBafudaTransform.childCount;
    }
    void AddCardToBafuda(int cardIndex)
    {
        int slot = GetFirstEmptySlot();
        if (slot == -1) return; // 空きなし

        GameObject newBa = Instantiate(bafudaPrefab, parentBafudaTransform);

        // スロット位置に配置
        newBa.transform.localPosition = GetBafudaSlotPosition(slot);
        newBa.transform.localRotation = Quaternion.Euler(0, -90, 0);

        // index と Material
        newBa.GetComponent<CardInfo>().cardIndex = cardIndex;
        newBa.transform.Find("Flont").GetComponent<Renderer>().material = cardMaterials[cardIndex];

        // ★ スロットに登録
        bafudaSlots[slot] = newBa;
    }
    Vector3 GetBafudaSlotPosition(int slot)
    {
        if (slot < 4)
        {
            return new Vector3(0.45f, 0.465f, 1.1f - (slot * 0.5f));
        }
        else
        {
            return new Vector3(-0.45f, 0.465f, 1.1f - ((slot - 4) * 0.5f));
        }
    }
    int GetFirstEmptySlot()
    {
        for (int i = 0; i < bafudaSlots.Length; i++)
        {
            if (bafudaSlots[i] == null)
                return i;
        }
        return -1; // 空きなし
    }

    void drowNextYamafuda()
    {
        selectYamafudaObj = Instantiate(yamafudaPrefab, parentDrowTefuda);
        selectYamafudaObj.transform.localPosition = new Vector3(0, 0.545f, 1.7f);
        selectYamafudaObj.transform.localRotation = Quaternion.Euler(0, -90, 0);

        // ★ ここで index を決める
        int idxMy = GetNextYamafuda();
        selectYamafudaObj.GetComponent<CardInfo>().cardIndex = idxMy;
        Debug.Log(idxMy);

        // Material を貼る
        Transform flont = selectYamafudaObj.transform.Find("Flont");
        Renderer rend = flont.GetComponent<Renderer>();
        rend.material = cardMaterials[idxMy];
    }

    void insSelectTefuda()
    {
        selectObj = Instantiate(insTefudaPrefab, parentInsTefuda);
        selectObj.transform.localPosition = new Vector3(0, 0f, 0f);
        selectObj.transform.localRotation = Quaternion.Euler(0, -90, 0);

        // Material を貼る
        Transform flont = selectObj.transform.Find("Flont");
        Renderer rend = flont.GetComponent<Renderer>();
        rend.material = cardMaterials[selectCardNum];
    }
}