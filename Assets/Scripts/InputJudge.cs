using UnityEngine;

public class InputJudge : MonoBehaviour
{
    public BeatmapData beatmap;
    public JudgementManager judgementManager; // reference to the player's manager

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

    //void JudgeHit()
    //{
    //    float songPos = conductor.songPosition;
    //    float nearestBeat = conductor.GetNearestBeatTime(songPos);
    //    float diff = Mathf.Abs(songPos - nearestBeat);

    //    string result = "Bad";

    //    if (diff <= perfectWindow)
    //        result = "Perfect";
    //    else if (diff <= greatWindow)
    //        result = "Good";
    //    else if (diff <= goodWindow)
    //        result = "Bad";

    //    judgementManager.ShowJudgement(result);
    //}

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
        string result = "Bad";

        if (closest <= perfectWindow)
            result = "Perfect";
        else if (closest <= greatWindow)
            result = "Good";
        else if (closest <= goodWindow)
            result = "Bad";

        judgementManager.ShowJudgement(result);
        //judgementManager.ShowJudgement
        Debug.Log(result);
        Debug.Log(closest);
        //if (closest <= perfectWindow)
        //{
        //    Debug.Log("Perfect");
        //}
        //else if (closest <= greatWindow)
        //{
        //    Debug.Log("Great");
        //}
        //else if (closest <= goodWindow)
        //{
        //    Debug.Log("Good");
        //}
        //else
        //{
        //    Debug.Log("Bad");
        //}
    }
}