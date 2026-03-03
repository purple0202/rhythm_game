using UnityEngine;

public class BeatManager : MonoBehaviour
{
    public float bpm = 120f;
    public AudioSource audioSource;

    public float beatInterval;
    public float songPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        beatInterval = 60f / bpm;
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        songPosition = audioSource.time;
    }

    public float GetBeatInterval()
    {
        return beatInterval;
    }

    public float GetSongTime()
    {
        return songPosition;
    }
}
