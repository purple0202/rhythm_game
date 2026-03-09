using UnityEngine;
using System.Collections;

public class JudgementManager : MonoBehaviour
{
    public GameObject perfectPrefab;
    public GameObject goodPrefab;
    public GameObject badPrefab;
    int currentOrder = 0;

    public float displayTime = 0.5f;

    public void ShowJudgement(string judgement)
    {
        //Debug.Log("JudgementManager is on: " + gameObject.name);
        //Debug.Log("Perfect prefab: " + perfectPrefab);
        //Debug.Log("Perfect prefab: " + perfectPrefab);
        //Debug.Log("Good prefab: " + goodPrefab);
        //Debug.Log("Bad prefab: " + badPrefab);
        GameObject prefab = null;
        switch (judgement)
        {
            case "Perfect": prefab = perfectPrefab; break;
            case "Good": prefab = goodPrefab; break;
            case "Bad": prefab = badPrefab; break;
        }
        Debug.Log("reached here!");
        if (prefab != null)
        {
            // Spawn a new instance slightly above player
            
            SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
            sr.sortingLayerName = "Foreground";  // a layer above Player
            sr.sortingOrder = 10;
            Vector3 spawnPos = transform.position + new Vector3(0.25f, 0.25f, 0);
            GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity);
            SpriteRenderer sre = instance.GetComponent<SpriteRenderer>();
            sre.sortingOrder = currentOrder;
            currentOrder++;
            instance.transform.parent = transform; // optional: move with player
            Debug.Log("reached!");
            StartCoroutine(HideAfterTime(instance, displayTime));
        }
    }

    private IEnumerator HideAfterTime(GameObject obj, float time)
    {
        obj.SetActive(true);
        yield return new WaitForSeconds(time);
        Destroy(obj);
    }
}
//using UnityEngine;
//using System.Collections;

//public class JudgementManager : MonoBehaviour
//{
//    public GameObject perfectSprite;
//    public GameObject goodSprite;
//    public GameObject badSprite;

//    public float displayTime = 0.5f;

//    public void ShowJudgement(string judgement)
//    {
//        GameObject obj = null;

//        switch (judgement)
//        {
//            case "Perfect":
//                obj = perfectSprite;
//                break;
//            case "Good":
//                obj = goodSprite;
//                break;
//            case "Bad":
//                obj = badSprite;
//                break;
//        }

//        if (obj != null)
//        {
//            StartCoroutine(Display(obj));
//        }
//    }

//    private IEnumerator Display(GameObject obj)
//    {
//        obj.SetActive(true);
//        yield return new WaitForSeconds(displayTime);
//        obj.SetActive(false);
//    }
//}
