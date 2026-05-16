using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SuperBattleManager : MonoBehaviour
{
    public Camera cam;
    public GameObject yamafudaPrefab;
    public GameObject myTefudaPrefab;
    public GameObject yourTefudaPrefab;
    public GameObject bafudaPrefab;
    public GameObject cardPrefab;
    public GameObject insTefudaPrefab;
    public GameObject insYourTefudaPrefab;

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
    public Transform parentHikariTransform2;
    public Transform parentTaneTransform2;
    public Transform parentTanzakuTransform2;
    public Transform parentKasuTransform2;
    public Transform parentDrowTefuda;
    public Transform parentInsTefuda;
    private YakuType lastYaku = YakuType.None;

    private int yamafudaCount = 8;
    private int tefudaCount = 8;
    private bool hasKoikoi = false;

    private int selectCardNum = 0;
    private GameObject selectObj = null;
    private GameObject selectYamafudaObj = null;

    private int gameNum = 3;
    private int currentGameNum = 1;

    public Material[] cardMaterials;
    List<int> numbers = new List<int>();
    List<int> emptyBafudaSlots = new List<int>();
    GameObject[] bafudaSlots = new GameObject[8];
    Queue<int> q;

    private YakuType yaku;
    private bool oyaFlag = false;

    public static bool isTurn = false;
    private bool hasCheckedThisTurn = false;
    public static bool isYamafudaDrawing = false;
    private bool isSelectTefuda = false;

    public List<BafudaScript> fieldCards = new List<BafudaScript>();
    public static bool turnChanging = true;
    private bool turnChange = false;
    private int yamafudaNum = 0;

    public GameObject koikoiPanel;
    public Text myScoreText;
    public Text yourScoreText;
    public Text turnText;

    // ゲーム終了時のリザルト表示
    public GameObject resultPanel;
    public Text resultDetailText;
    public Text resultTotalText;

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

    private int[] myCardTypeCount = { 0, 0, 0, 0 };
    private int[] yourCardTypeCount = { 0, 0, 0, 0 };

    private int[,] myMochifuda = {
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
    };

    private int[,] yourMochifuda = {
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
    };

    [System.Flags]
    public enum YakuType
    {
        None = 0,
        Kasu10 = 1 << 0,   // 1
        Tan5 = 1 << 1,   // 2
        Tane5 = 1 << 2,   // 4
        Sankou = 1 << 3,   // 8
        Shikou = 1 << 4,   // 16
        Gokou = 1 << 5,   // 32
        Akatan = 1 << 6,   // 64
        Aotan = 1 << 7,   // 128
        Inoshikachou = 1 << 8,   // 256
        Tsukimizake = 1 << 9,   // 512
        Hanamizake = 1 << 10,  // 1024
        Ameshikou = 1 << 11,   // 2048
    }

    private void OnEnable()
    {
        oyaFlag = isTurn;
        koikoiPanel.SetActive(false);
        myScoreText.text = "0";
        yourScoreText.text = "0";
        if (isTurn)
        {
            turnText.text = "自分の手番";
        }
        else
        {
            turnText.text = "相手の手番";
        }
        if (cam == null)
        {
            cam = Camera.main;
        }
        instanceYamafudaCards();
        StartCoroutine(devideTefudaCards());
    }
    void Start()
    {
        /*koikoiPanel.SetActive(false);
        myScoreText.text = "0";
        yourScoreText.text = "0";
        turnText.text = "じぶんの手番";
        if (cam == null)
        {
            cam = Camera.main;
        }
        instanceYamafudaCards();
        StartCoroutine(devideTefudaCards());*/
    }

    void Update()
    {
        if (!turnChanging)
        {
            //Debug.Log("停止確認中");
            if (AllCardsStopped())
            {
                turnChanging = true;
                Debug.Log("札停止");
                if (isYamafudaDrawing)
                {
                    //役の確認
                    yaku = CheckYaku();

                    if (yaku != 0)
                    {
                        // ★ 倍返し判定（自分がこいこい中 & 相手手番で役成立）
                        if (!isTurn && hasKoikoi)
                        {
                            Debug.Log("倍返し発動！相手が役を作ったので即終了");

                            int kasuCount, tanCount, taneCount, hikariCount;
                            GetCurrentCounts(out kasuCount, out tanCount, out taneCount, out hikariCount);

                            int score = CalculateYakuScore(yaku, kasuCount, tanCount, taneCount);

                            // 倍返し → 2倍
                            score *= 2;

                            StartCoroutine(ShowResultCoroutine(score));
                            return;
                        }

                        // ここから下は今のままでOK
                        YakuType newYaku = yaku & ~lastYaku;

                        if (newYaku != 0)
                        {
                            if (isTurn)
                            {
                                koikoiPanel.SetActive(true);
                            }
                        }
                        else
                        {
                            isTurn = !isTurn;
                            if (turnText.text == "自分の手番")
                            {
                                turnText.text = "相手の手番";
                                insSelectTefuda();
                            }
                            else
                            {
                                turnText.text = "自分の手番";
                            }
                        }

                        lastYaku = yaku;
                    }
                    else
                    {
                        // 役が無かったら手番変更
                        isTurn = !isTurn;

                        if (turnText.text == "自分の手番")
                        {
                            turnText.text = "相手の手番";
                            insSelectTefuda();
                        }
                        else
                        {
                            turnText.text = "自分の手番";
                        }
                    }

                    isYamafudaDrawing = false;
                    isSelectTefuda = false;
                }
                else
                {
                    drowNextYamafuda();
                    isYamafudaDrawing = true;
                    Debug.Log("山札召喚");
                }
            }
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
                    Debug.Log("isTurn:" + isTurn);
                    if (!isTurn || isYamafudaDrawing || isSelectTefuda) return;

                    CardInfo info = obj.GetComponent<CardInfo>();
                    if (info == null) return;

                    int handIndex = info.cardIndex;
                    selectCardNum = handIndex;
                    isSelectTefuda = true;

                    Transform objPosition = obj.transform;

                    // 空中に手札を生成
                    insSelectTefuda();
                    Destroy(obj);

                    // 選択した手札の位置に次の札を生成
                    /*if (yamafudaNum < cardMaterials.Length - 1)
                    {
                        GameObject myObj = Instantiate(myTefudaPrefab, parentMyTefudaTransform);
                        myObj.transform.localPosition = objPosition.position;
                        myObj.transform.localRotation = Quaternion.Euler(0, -90, 0);

                        // ★ ここで index を決める
                        int idxMy = GetNextYamafuda();
                        myObj.GetComponent<CardInfo>().cardIndex = idxMy;

                        // Material を貼る
                        Transform flont = myObj.transform.Find("Flont");
                        Renderer rend = flont.GetComponent<Renderer>();
                        rend.material = cardMaterials[idxMy];
                    }*/

                    return;
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
        numbers.Clear();
        for(int i = 0; i < myMochifuda.GetLength(0); i++)
        {
            for(int j = 0; j < myMochifuda.GetLength(1); j++)
            {
                myMochifuda[i, j] = 0;
                yourMochifuda[i, j] = 0;
            }
        }
        for(int i= 0; i < myCardTypeCount.Length; i++)
        {
            myCardTypeCount[i] = 0;
            yourCardTypeCount[i] = 0;
        }
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

        foreach (Transform child in parentBafudaTransform)
        {
            var script = child.GetComponent<BafudaScript>();
            if (script != null)
                fieldCards.Add(script);
        }
        if (!isTurn)
        {
            insSelectTefuda();
        }
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
        //Debug.Log("---- 場札の状態 ----");
        foreach (Transform child in parentBafudaTransform)
        {
            CardInfo info = child.GetComponent<CardInfo>();
            if (info == null)
            {
                //Debug.Log("Bafuda child に CardInfo が付いてない");
                continue;
            }

            int idx = info.cardIndex;
            //Debug.Log("場札 index = " + idx + " / 月 = " + cardMonth[idx] + " / タイプ = " + cardType[idx]);
        }
        //Debug.Log("---- ここまで ----");

        List<int> matches = GetMatchCardIndexes();

        if (matches.Count == 0)
        {
            //Debug.Log("一致する手札（月）はありません");
        }
        else
        {
            //Debug.Log("一致する手札のカード：");
            foreach (int idx in matches)
            {
                //Debug.Log("手札 index = " + idx + " / 月 = " + cardMonth[idx] + " / タイプ = " + cardType[idx]);
            }
        }
    }

    int GetNextYamafuda()
    {
        yamafudaNum++;
        Debug.Log("yamahuda残り" + (48 - yamafudaNum));
        return q.Dequeue();
    }

    /*int GetFirstEmptyBafudaSlot()
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
    }*/

    /*void AddCardToBafuda(int cardIndex)
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
    }*/
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
    /*int GetFirstEmptySlot()
    {
        for (int i = 0; i < bafudaSlots.Length; i++)
        {
            if (bafudaSlots[i] == null)
                return i;
        }
        return -1; // 空きなし
    }*/

    void drowNextYamafuda()
    {
        if (isTurn)
        {
            selectYamafudaObj = Instantiate(insTefudaPrefab, parentInsTefuda);
            selectYamafudaObj.transform.localPosition = new Vector3(0, 0f, 0f);
            selectYamafudaObj.transform.localRotation = Quaternion.Euler(0, -90, 0);
        }
        else
        {
            // ★ ランダム位置を作成
            float randX = Random.Range(-0.67f, 0.67f);
            float randZ = Random.Range(-1.8f, 1.8f);
            Vector3 spawnPos = new Vector3(randX, 0f, randZ);

            selectYamafudaObj = Instantiate(insYourTefudaPrefab, parentInsTefuda);
            selectYamafudaObj.transform.localPosition = spawnPos;
            selectYamafudaObj.transform.localRotation = Quaternion.Euler(0, 90, 0);
        }

        if (yamafudaNum < cardMaterials.Length)
        {
            // ★ ここで index を決める
            int idxMy = GetNextYamafuda();
            selectYamafudaObj.GetComponent<CardInfo>().cardIndex = idxMy;
            Debug.Log(idxMy);

            // Material を貼る
            Transform flont = selectYamafudaObj.transform.Find("Flont");
            Renderer rend = flont.GetComponent<Renderer>();
            rend.material = cardMaterials[idxMy];
        }
    }

    void insSelectTefuda()
    {
        if (isTurn)
        {
            selectObj = Instantiate(insTefudaPrefab, parentInsTefuda);
            selectObj.transform.localPosition = new Vector3(0, 0f, 0f);
            selectObj.transform.localRotation = Quaternion.Euler(0, -90, 0);

            selectObj.GetComponent<CardInfo>().cardIndex = selectCardNum;
            Debug.Log(selectCardNum);

            // Material を貼る
            Transform flont = selectObj.transform.Find("Flont");
            Renderer rend = flont.GetComponent<Renderer>();
            rend.material = cardMaterials[selectCardNum];
        }
        else
        {
            // 相手の手札を取得
            int count = parentYourTefudaTransform.childCount;
            if (count == 0)
            {
                Debug.Log("相手の手札がありません");
                return;
            }
            // ランダムに1枚選ぶ
            int r = Random.Range(0, count);
            Transform yourCard = parentYourTefudaTransform.GetChild(r);
            Transform nextTefuda = yourCard.transform;

            // カード情報を取得
            CardInfo info = yourCard.GetComponent<CardInfo>();
            if (info == null)
            {
                Debug.LogError("相手の手札に CardInfo が付いていません");
                return;
            }
            int idx = info.cardIndex;

            // ★ ランダム位置を作成
            float randX = Random.Range(-0.67f, 0.67f);
            float randZ = Random.Range(-1.8f, 1.8f);
            Vector3 spawnPos = new Vector3(randX, 0f, randZ);

            // insTefuda を生成
            GameObject obj = Instantiate(insYourTefudaPrefab, parentInsTefuda);
            obj.transform.localPosition = spawnPos;
            obj.transform.localRotation = Quaternion.Euler(0, 90, 0);

            // index をセット
            obj.GetComponent<CardInfo>().cardIndex = idx;

            // 絵柄を貼る
            Transform flont = obj.transform.Find("Flont");
            Renderer rend = flont.GetComponent<Renderer>();
            rend.material = cardMaterials[idx];

            Debug.Log("相手の手札からランダムに1枚生成 → index = " + idx);

            // ★ 相手の手札から削除（重要）
            Destroy(yourCard.gameObject);

            // 引いた手札の位置に次の山札を生成
            /*if (yamafudaNum < cardMaterials.Length)
            {
                GameObject obj2 = Instantiate(yourTefudaPrefab, parentYourTefudaTransform);
                obj2.transform.localPosition = nextTefuda.position;
                obj2.transform.localRotation = Quaternion.Euler(0, -90, 180);

                // index をセット
                int idxYour = GetNextYamafuda();
                obj2.GetComponent<CardInfo>().cardIndex = idxYour;

                // 絵柄を貼る
                Transform flont2 = obj2.transform.Find("Flont");
                Renderer rend2 = flont2.GetComponent<Renderer>();
                rend2.material = cardMaterials[idxYour];
            }*/
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        CardInfo info = collision.gameObject.GetComponent<CardInfo>();
        if (info == null) return;

        int baIndex = info.cardIndex;

        if (collision.collider.CompareTag("Bafuda") || collision.collider.CompareTag("insTefuda"))
        {
            //Debug.Log("Floor と Bafuda が接触した！");
            GameObject obj2 = null;

            if (isTurn)
            {
                // ★ 場札 → 持ち札
                if (cardType[baIndex] == 1)
                {
                    myMochifuda[cardType[baIndex] - 1, myCardTypeCount[0]] = baIndex + 1;
                    obj2 = Instantiate(yamafudaPrefab, parentHikariTransform);
                    obj2.transform.localPosition = new Vector3(0.55f, 0.465f + (myCardTypeCount[0] * 0.01f), -2.35f - (myCardTypeCount[0] * 0.1f));
                    obj2.transform.localRotation = Quaternion.Euler(0, -90, 0);
                    myCardTypeCount[0]++;
                }
                else if (cardType[baIndex] == 2)
                {
                    myMochifuda[cardType[baIndex] - 1, myCardTypeCount[1]] = baIndex + 1;
                    obj2 = Instantiate(yamafudaPrefab, parentTaneTransform);
                    obj2.transform.localPosition = new Vector3(-0.15f, 0.465f + (myCardTypeCount[1] * 0.01f), -2.35f - (myCardTypeCount[1] * 0.1f));
                    obj2.transform.localRotation = Quaternion.Euler(0, -90, 0);
                    myCardTypeCount[1]++;
                }
                else if (cardType[baIndex] == 3)
                {
                    myMochifuda[cardType[baIndex] - 1, myCardTypeCount[2]] = baIndex + 1;
                    obj2 = Instantiate(yamafudaPrefab, parentTanzakuTransform);
                    obj2.transform.localPosition = new Vector3(-0.85f, 0.465f + (myCardTypeCount[2] * 0.01f), -2.35f - (myCardTypeCount[2] * 0.1f));
                    obj2.transform.localRotation = Quaternion.Euler(0, -90, 0);
                    myCardTypeCount[2]++;
                }
                else if (cardType[baIndex] == 4)
                {
                    myMochifuda[cardType[baIndex] - 1, myCardTypeCount[3]] = baIndex + 1;
                    obj2 = Instantiate(yamafudaPrefab, parentKasuTransform);
                    obj2.transform.localPosition = new Vector3(-1.55f, 0.465f + (myCardTypeCount[3] * 0.01f), -2.35f - (myCardTypeCount[3] * 0.1f));
                    obj2.transform.localRotation = Quaternion.Euler(0, -90, 0);
                    myCardTypeCount[3]++;
                }
            }
            else
            {
                // ★ 場札 → 持ち札
                if (cardType[baIndex] == 1)
                {
                    yourMochifuda[cardType[baIndex] - 1, yourCardTypeCount[0]] = baIndex + 1;
                    obj2 = Instantiate(yamafudaPrefab, parentHikariTransform2);
                    obj2.transform.localPosition = new Vector3(-0.55f, 0.465f + (yourCardTypeCount[0] * 0.01f), 3.25f - (yourCardTypeCount[0] * 0.1f));
                    obj2.transform.localRotation = Quaternion.Euler(0, 90, 0);
                    yourCardTypeCount[0]++;
                }
                else if (cardType[baIndex] == 2)
                {
                    yourMochifuda[cardType[baIndex] - 1, yourCardTypeCount[1]] = baIndex + 1;
                    obj2 = Instantiate(yamafudaPrefab, parentTaneTransform2);
                    obj2.transform.localPosition = new Vector3(0.15f, 0.465f + (yourCardTypeCount[1] * 0.01f), 3.25f - (yourCardTypeCount[1] * 0.1f));
                    obj2.transform.localRotation = Quaternion.Euler(0, 90, 0);
                    yourCardTypeCount[1]++;
                }
                else if (cardType[baIndex] == 3)
                {
                    yourMochifuda[cardType[baIndex] - 1, yourCardTypeCount[2]] = baIndex + 1;
                    obj2 = Instantiate(yamafudaPrefab, parentTanzakuTransform2);
                    obj2.transform.localPosition = new Vector3(0.85f, 0.465f + (yourCardTypeCount[2] * 0.01f), 3.25f - (yourCardTypeCount[2] * 0.1f));
                    obj2.transform.localRotation = Quaternion.Euler(0, 90, 0);
                    yourCardTypeCount[2]++;
                }
                else if (cardType[baIndex] == 4)
                {
                    yourMochifuda[cardType[baIndex] - 1, yourCardTypeCount[3]] = baIndex + 1;
                    obj2 = Instantiate(yamafudaPrefab, parentKasuTransform2);
                    obj2.transform.localPosition = new Vector3(1.55f, 0.465f + (yourCardTypeCount[3] * 0.01f), 3.25f - (yourCardTypeCount[3] * 0.1f));
                    obj2.transform.localRotation = Quaternion.Euler(0, 90, 0);
                    yourCardTypeCount[3]++;
                }
            }

            obj2.GetComponent<CardInfo>().cardIndex = baIndex;

            // ★ Material 設定
            obj2.transform.Find("Flont").GetComponent<Renderer>().material = cardMaterials[baIndex];

            BafudaScript bafuda = collision.gameObject.GetComponent<BafudaScript>();
            if (bafuda != null)
            {
                fieldCards.Remove(bafuda);
            }
            Destroy(collision.gameObject);

            //isSelectTefuda = false;

        }
    }
    private YakuType CheckYaku()
    {
        int kasuCount = 0;
        int tanCount = 0;
        int taneCount = 0;
        int hikariCount = 0;

        YakuType result = YakuType.None;

        // --- 種類ごとの枚数カウント（今の構造でOK） ---
        for (int i = 0; i < 12; i++)
        {
            int[,] target = isTurn ? myMochifuda : yourMochifuda;

            if (target[3, i] > 0) kasuCount++;
            if (target[2, i] > 0) tanCount++;
            if (target[1, i] > 0) taneCount++;
            if (target[0, i] > 0) hikariCount++;
        }

        // --- 基本役 ---
        if (kasuCount >= 10) result |= YakuType.Kasu10;
        if (tanCount >= 5) result |= YakuType.Tan5;
        if (taneCount >= 5) result |= YakuType.Tane5;

        if (hikariCount == 3) result |= YakuType.Sankou;
        else if (hikariCount == 4) result |= YakuType.Shikou;
        else if (hikariCount == 5) result |= YakuType.Gokou;

        // --- ここから cardIndex を使った正しい役判定 ---
        List<int> cards = GetMochifudaCardIndexes(isTurn);
        DebugMochifuda(cards);

        // 赤短
        bool hasAka1 = cards.Any(i => cardMonth[i] == 1 && cardType[i] == 3);
        bool hasAka2 = cards.Any(i => cardMonth[i] == 2 && cardType[i] == 3);
        bool hasAka3 = cards.Any(i => cardMonth[i] == 3 && cardType[i] == 3);
        if (hasAka1 && hasAka2 && hasAka3)
            result |= YakuType.Akatan;

        // 青短
        bool hasAo1 = cards.Any(i => cardMonth[i] == 6 && cardType[i] == 3);
        bool hasAo2 = cards.Any(i => cardMonth[i] == 9 && cardType[i] == 3);
        bool hasAo3 = cards.Any(i => cardMonth[i] == 10 && cardType[i] == 3);
        if (hasAo1 && hasAo2 && hasAo3)
            result |= YakuType.Aotan;

        // 猪鹿蝶
        bool hasCho = cards.Any(i => cardMonth[i] == 6 && cardType[i] == 2);
        bool hasIno = cards.Any(i => cardMonth[i] == 7 && cardType[i] == 2);
        bool hasShika = cards.Any(i => cardMonth[i] == 10 && cardType[i] == 2);
        if (hasCho && hasIno && hasShika)
            result |= YakuType.Inoshikachou;

        // 月見酒
        bool hasTsuki = cards.Any(i => cardMonth[i] == 8 && cardType[i] == 1);
        bool hasSakazuki = cards.Any(i => cardMonth[i] == 9 && cardType[i] == 2);
        if (hasTsuki && hasSakazuki)
            result |= YakuType.Tsukimizake;

        // 花見酒
        bool hasSakuraHikari = cards.Any(i => cardMonth[i] == 3 && cardType[i] == 1);
        if (hasSakuraHikari && hasSakazuki)
            result |= YakuType.Hanamizake;

        return result;
    }
    private int CalculateYakuScore(YakuType yaku, int kasuCount, int tanCount, int taneCount)
    {
        int score = 0;

        // --- カス10 ---
        if ((yaku & YakuType.Kasu10) != 0)
        {
            score += 1 + (kasuCount - 10); // 10枚で1点、以降1枚ごとに+1
            Debug.Log("カス10");
        }

        // --- タン5 ---
        if ((yaku & YakuType.Tan5) != 0)
        {
            score += 1 + (tanCount - 5);
            Debug.Log("タン5");
        }

        // --- タネ5 ---
        if ((yaku & YakuType.Tane5) != 0)
        {
            score += 1 + (taneCount - 5);
            Debug.Log("タネ5");
        }

        // --- 赤短 ---
        if ((yaku & YakuType.Akatan) != 0)
        {
            score += 5;
            Debug.Log("赤短");
        }

        // --- 青短 ---
        if ((yaku & YakuType.Aotan) != 0)
        {
            score += 5;
            Debug.Log("青短");
        }

        // --- 猪鹿蝶 ---
        if ((yaku & YakuType.Inoshikachou) != 0)
        {
            score += 5;
            Debug.Log("猪鹿蝶");
        }

        // --- 三光 ---
        if ((yaku & YakuType.Sankou) != 0)
        {
            score += 5;
            Debug.Log("三光");
        }

        // --- 四光 ---
        if ((yaku & YakuType.Shikou) != 0)
        {
            score += 8;
            Debug.Log("四光");
        }

        // --- 五光 ---
        if ((yaku & YakuType.Gokou) != 0)
        {
            score += 10;
            Debug.Log("五光");
        }

        // --- 月見酒 ---
        if ((yaku & YakuType.Tsukimizake) != 0)
        {
            score += 5;
            Debug.Log("月見酒");
        }

        // --- 花見酒 ---
        if ((yaku & YakuType.Hanamizake) != 0)
        {
            score += 5;
            Debug.Log("花見酒");
        }

        return score;
    }
    bool AllCardsStopped()
    {
        fieldCards.RemoveAll(card => card == null);

        foreach (var card in fieldCards)
        {
            if (!card.isStopped)
                return false;
        }
        return true;
    }

    public void OnKoikoi(int num)
    {
        // こいこいしたら手番変更
        if(num == 0)
        {
            hasKoikoi = true;
            turnChange = true;
            isTurn = !isTurn;
            if (isTurn)
            {
                turnText.text = "自分の手番";
            }
            else
            {
                turnText.text = "相手の手番";
                insSelectTefuda();
            }
            Debug.Log("手番変更");
            
        }
        // こいこいしなかったら点数をもらって次のゲームへ
        else if(num == 1)
        {
            // カス・短冊・タネの枚数を取得
            int kasuCount = 0;
            int tanCount = 0;
            int taneCount = 0;

            for (int i = 0; i < 12; i++)
            {
                if (isTurn)
                {
                    if (myMochifuda[3, i] > 0) kasuCount++;
                    if (myMochifuda[2, i] > 0) tanCount++;
                    if (myMochifuda[1, i] > 0) taneCount++;
                }
                else
                {
                    if (yourMochifuda[3, i] > 0) kasuCount++;
                    if (yourMochifuda[2, i] > 0) tanCount++;
                    if (yourMochifuda[1, i] > 0) taneCount++;
                }
            }

            // 点数計算
            int score = CalculateYakuScore(yaku, kasuCount, tanCount, taneCount);

            // こいこいしていたら倍々
            if (hasKoikoi)
            {
                score *= 2;
            }

            if (isTurn)
            {
                myScoreText.text = "" + score; 
            }
            else
            {
                yourScoreText.text = "" + score;
            }


            Debug.Log(score + "点獲得！！！");

            StartCoroutine(ShowResultCoroutine(score));

        }
        koikoiPanel.SetActive(false);
    }

    Dictionary<string, int> GetYakuScoreDetail(YakuType yaku, int kasuCount, int tanCount, int taneCount)
    {
        Dictionary<string, int> detail = new Dictionary<string, int>();

        // カス10
        if ((yaku & YakuType.Kasu10) != 0)
        {
            int s = 1 + (kasuCount - 10);
            detail.Add("カス", s);
        }

        // タン5
        if ((yaku & YakuType.Tan5) != 0)
        {
            int s = 1 + (tanCount - 5);
            detail.Add("短冊", s);
        }

        // タネ5
        if ((yaku & YakuType.Tane5) != 0)
        {
            int s = 1 + (taneCount - 5);
            detail.Add("タネ", s);
        }

        if ((yaku & YakuType.Sankou) != 0) detail.Add("三光", 5);
        if ((yaku & YakuType.Shikou) != 0) detail.Add("四光", 8);
        if ((yaku & YakuType.Gokou) != 0) detail.Add("五光", 10);

        if ((yaku & YakuType.Akatan) != 0) detail.Add("赤短", 5);
        if ((yaku & YakuType.Aotan) != 0) detail.Add("青短", 5);
        if ((yaku & YakuType.Inoshikachou) != 0) detail.Add("猪鹿蝶", 5);

        if ((yaku & YakuType.Tsukimizake) != 0) detail.Add("月見酒", 5);
        if ((yaku & YakuType.Hanamizake) != 0) detail.Add("花見酒", 5);

        return detail;
    }

    private List<int> GetMochifudaCardIndexes(bool isMyTurn)
    {
        List<int> result = new List<int>();

        Transform[] parents = isMyTurn
            ? new Transform[] { parentHikariTransform, parentTaneTransform, parentTanzakuTransform, parentKasuTransform }
            : new Transform[] { parentHikariTransform2, parentTaneTransform2, parentTanzakuTransform2, parentKasuTransform2 };

        foreach (Transform p in parents)
        {
            foreach (Transform child in p)
            {
                CardInfo info = child.GetComponent<CardInfo>();
                if (info != null)
                    result.Add(info.cardIndex);
            }
        }

        return result;
    }
    private void DebugMochifuda(List<int> cards)
    {
        foreach (int i in cards)
        {
            Debug.Log($"idx:{i}, month:{cardMonth[i]}, type:{cardType[i]}");
        }
    }

    private void resetTheGame()
    {
        hasKoikoi = false;
        yamafudaNum = 0;
        foreach (Transform child in parentBafudaTransform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in parentMyTefudaTransform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in parentYourTefudaTransform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in parentHikariTransform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in parentTaneTransform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in parentTanzakuTransform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in parentKasuTransform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in parentHikariTransform2)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in parentTaneTransform2)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in parentTanzakuTransform2)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in parentKasuTransform2)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in parentInsTefuda)
        {
            Destroy(child.gameObject);
        }
    }
    // ★ 今のプレイヤーの持ち札から枚数カウントだけ取り出す
    void GetCurrentCounts(out int kasuCount, out int tanCount, out int taneCount, out int hikariCount)
    {
        kasuCount = 0;
        tanCount = 0;
        taneCount = 0;
        hikariCount = 0;

        int[,] target = isTurn ? myMochifuda : yourMochifuda;

        for (int i = 0; i < 12; i++)
        {
            if (target[3, i] > 0) kasuCount++;
            if (target[2, i] > 0) tanCount++;
            if (target[1, i] > 0) taneCount++;
            if (target[0, i] > 0) hikariCount++;
        }
    }
    void StartNextGame()
    {
        resetTheGame();
        StartCoroutine(devideTefudaCards());
        oyaFlag = !oyaFlag;
        isTurn = !oyaFlag;
    }
    string YakuToString(YakuType y)
    {
        List<string> list = new List<string>();

        if ((y & YakuType.Kasu10) != 0) list.Add("カス10");
        if ((y & YakuType.Tan5) != 0) list.Add("タン5");
        if ((y & YakuType.Tane5) != 0) list.Add("タネ5");
        if ((y & YakuType.Sankou) != 0) list.Add("三光");
        if ((y & YakuType.Shikou) != 0) list.Add("四光");
        if ((y & YakuType.Gokou) != 0) list.Add("五光");
        if ((y & YakuType.Akatan) != 0) list.Add("赤短");
        if ((y & YakuType.Aotan) != 0) list.Add("青短");
        if ((y & YakuType.Inoshikachou) != 0) list.Add("猪鹿蝶");
        if ((y & YakuType.Tsukimizake) != 0) list.Add("月見酒");
        if ((y & YakuType.Hanamizake) != 0) list.Add("花見酒");

        if (list.Count == 0) return "役なし";

        return string.Join(" / ", list);
    }
    IEnumerator ShowResultCoroutine(int score)
    {
        resultPanel.SetActive(true);

        // 今の持ち札から枚数カウント
        int kasuCount, tanCount, taneCount, hikariCount;
        GetCurrentCounts(out kasuCount, out tanCount, out taneCount, out hikariCount);

        // 役ごとの点数を取得
        var detail = GetYakuScoreDetail(lastYaku, kasuCount, tanCount, taneCount);

        // 表示用文字列を作成
        string text = "";
        foreach (var d in detail)
        {
            text += $"・{d.Key} …… {d.Value}文\n";
        }

        resultDetailText.text = text;
        resultTotalText.text = $"合計：{score}文";

        yield return new WaitForSeconds(2.0f);

        StartNextGame();
    }

    public void OnCloseResult()
    {
        resultPanel.SetActive(false);
    }
}