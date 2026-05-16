using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OyagimeScript : MonoBehaviour
{

    public Transform parentOyagimeTransform;
    public GameObject oyagimeFudaPrefab;
    Queue<int> q;
    public Material[] cardMaterials;
    List<int> numbers = new List<int>();
    public Camera cam;
    private GameObject[] oyagimeFudas = new GameObject[3];
    private bool isIns = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.GetComponent<SuperBattleManager>().enabled = false;
        StartCoroutine(Oyagime());
    }

    // Update is called once per frame
    void Update()
    {
        if (!isIns) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject obj = hit.collider.gameObject;

                // ============================
                // ★ 手札クリック（CardInfo 方式）
                // ============================
                if (obj.CompareTag("OyagimeFuda"))
                {
                    int mySelectMonth = 0;
                    int yourSelectMonth = 0;
                    mySelectMonth = obj.GetComponent<CardInfo>().cardIndex;

                    Vector3 rot = obj.transform.eulerAngles;
                    rot.z = 0f;
                    obj.transform.eulerAngles = rot;
                    Debug.Log(obj.gameObject.name);

                    for (int i = 0; i < oyagimeFudas.Length; i++)
                    {
                        if(obj == oyagimeFudas[i])
                        {
                            int r = Random.Range(0, 2);
                            if (i == 0)
                            {
                                yourSelectMonth = oyagimeFudas[r + 1].GetComponent<CardInfo>().cardIndex;
                                Vector3 rot2 = oyagimeFudas[r + 1].transform.eulerAngles;
                                rot2.z = 0f;
                                oyagimeFudas[r + 1].transform.eulerAngles = rot2;
                            }
                            else if(i == 1)
                            {
                                yourSelectMonth = oyagimeFudas[r * 2].GetComponent<CardInfo>().cardIndex;
                                Vector3 rot2 = oyagimeFudas[r * 2].transform.eulerAngles;
                                rot2.z = 0f;
                                oyagimeFudas[r * 2].transform.eulerAngles = rot2;
                            }
                            else
                            {
                                yourSelectMonth = oyagimeFudas[r].GetComponent<CardInfo>().cardIndex;
                                Vector3 rot2 = oyagimeFudas[r].transform.eulerAngles;
                                rot2.z = 0f;
                                oyagimeFudas[r].transform.eulerAngles = rot2;
                            }

                            break;
                        }
                    }

                    Debug.Log("my" + mySelectMonth + " your" + yourSelectMonth);
                    if(mySelectMonth > yourSelectMonth)
                    {
                        SuperBattleManager.isTurn = true;
                    }
                    else
                    {
                        SuperBattleManager.isTurn = false;
                    }

                    StartCoroutine(OnStartTheGame());
                }
            }
        }
    }

    IEnumerator Oyagime()
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

        for (int i = 0; i < 3; i++)
        {
            GameObject myObj = Instantiate(oyagimeFudaPrefab, parentOyagimeTransform);
            oyagimeFudas[i] = myObj;
            myObj.transform.localPosition = new Vector3(0f, 0f, 1f - i);
            myObj.transform.localRotation = Quaternion.Euler(0, -90, 180);

            // ★ ここで index を決める
            int idxMy = GetNextYamafuda();
            myObj.GetComponent<CardInfo>().cardIndex = idxMy;

            // Material を貼る
            Transform flont = myObj.transform.Find("Flont");
            Renderer rend = flont.GetComponent<Renderer>();
            rend.material = cardMaterials[idxMy];

            yield return new WaitForSeconds(0.2f);
        }
        isIns = true;
    }

    IEnumerator OnStartTheGame()
    {
        yield return new WaitForSeconds(3f);

        foreach (Transform child in parentOyagimeTransform)
        {
            Destroy(child.gameObject);
        }

        this.GetComponent<SuperBattleManager>().enabled = true;
    }
    int GetNextYamafuda()
    {
        return q.Dequeue();
    }
}
