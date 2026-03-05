using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Beatmap", menuName = "Rhythm/Beatmap")]
public class BeatmapData : ScriptableObject
{
    public AudioClip music;

    [Header("Song Info")]
    public float bpm;
    public float offset;

    [Header("Beat Times")]
    public List<float> beatTimings = new List<float>();
}