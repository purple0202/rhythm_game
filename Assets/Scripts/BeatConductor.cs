using UnityEngine;

public class BeatConductor : MonoBehaviour
{
    public static BeatConductor Instance;

    [Header("References")]
    public BeatmapData beatmap;
    public AudioSource audioSource;

    [Header("Song Timing")]
    public float songPosition;
    public float songPositionInBeats;

    private double songStartDSPTime;

    private float secondsPerBeat;

    [Header("Offsets")]
    public float songOffset = 0f;     // manual timing offset


    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        secondsPerBeat = 60f / beatmap.bpm;

        audioSource.clip = beatmap.music;

        beatmap.GenerateBeats();

        StartSong();
    }

    void StartSong()
    {
        double dspTime = AudioSettings.dspTime;

        double delay = 1.0; // give the audio system time to prepare

        songStartDSPTime = dspTime + delay;

        audioSource.PlayScheduled(songStartDSPTime);
    }

    void Update()
    {
        double dspTime = AudioSettings.dspTime;

        songPosition = (float)(dspTime - songStartDSPTime) + songOffset;

        songPositionInBeats = songPosition / secondsPerBeat;
    }
}