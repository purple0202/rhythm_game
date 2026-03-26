using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "Beatmap", menuName = "Rhythm/Beatmap")]
public class BeatmapData : ScriptableObject
{
    public AudioClip music;

    [Header("Song Info")]
    public float bpm;
    public float offset;
    public int songLength;

    //[Header("Beat Times")]
    //public List<float> beatTimings = new List<float>();

    //[Header("Beat Times")]
    //public List<float> beatTimings = new List<float>();
    //public List<int> ints = Enumerable.Range(1, song_length).ToList();
    //public float SecondsPerBeat => 60f / bpm;

    //public void GenerateBeats()
    //{
    //    for (int i = 0; i < beatTimings.Count; i++)
    //    {
    //        beatTimings[i] = offset + ints[i] * SecondsPerBeat;
    //    }
    //}

    [Header("Generated Beat Times")]
    public List<float> beatTimings = new List<float>();

    public float SecondsPerBeat => 60f / bpm;

    public void GenerateBeats()
    {
        beatTimings.Clear();

        float currentTime = offset;

        while (currentTime < songLength)
        {
            beatTimings.Add(currentTime);
            currentTime += SecondsPerBeat;
        }

        Debug.Log("Generated " + beatTimings.Count + " beats.");
    }

    public float GetLoopedTime(float songTime)
    {
        if (songLength <= 0) return songTime;

        return songTime % songLength;
    }



    public float GetBeatTime(int index)
    {
        return offset + index * SecondsPerBeat;
    }
}