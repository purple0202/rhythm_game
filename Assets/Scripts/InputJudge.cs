using UnityEngine;

public class InputJudge : MonoBehaviour
{
    public BeatmapData beatmap;

    public float perfectWindow = 0.05f;
    public float greatWindow = 0.1f;
    public float goodWindow = 0.15f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) || Input.GetMouseButtonDown(0))
        {
            CheckInput();
        }
    }

    void CheckInput()
    {
        float currentTime = BeatConductor.Instance.songPosition;

        float closest = float.MaxValue;

        foreach (float beat in beatmap.beatTimings)
        {
            float diff = Mathf.Abs(beat - currentTime);

            if (diff < closest)
                closest = diff;
        }

        if (closest <= perfectWindow)
        {
            Debug.Log("Perfect");
        }
        else if (closest <= greatWindow)
        {
            Debug.Log("Great");
        }
        else if (closest <= goodWindow)
        {
            Debug.Log("Good");
        }
        else
        {
            Debug.Log("Bad");
        }
    }
}