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

    //[Header("Beat Times")]
    //public List<float> beatTimings = new List<float>();

    //[Header("Beat Times")]
    public List<float> beatTimings = new List<float>();
    public List<int> ints = Enumerable.Range(1, 100).ToList();
    public float SecondsPerBeat => 60f / bpm;

    public void GenerateBeats()
    {
        for (int i = 0; i < beatTimings.Count; i++)
        {
            beatTimings[i] = offset + ints[i] * SecondsPerBeat;
        }
    }

    

    public float GetBeatTime(int index)
    {
        return offset + index * SecondsPerBeat;
    }
}