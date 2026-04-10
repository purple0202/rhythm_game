using UnityEngine;

public class BeatConductor : MonoBehaviour
{
    public static BeatConductor Instance;
    public static event System.Action OnBeat;

    [Header("References")]
    public BeatmapData beatmap;
    public AudioSource audioSource;

    [Header("Song Timing")]
    public float songPosition;
    public float songPositionInBeats;

    private double songStartDSPTime;
    private float secondsPerBeat;
    private int lastBeatIndex = -1;

    [Header("Offsets")]
    public float songOffset = 0f;

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
        double delay = 1.0;
        songStartDSPTime = dspTime + delay;
        audioSource.PlayScheduled(songStartDSPTime);
    }

    void Update()
    {
        double dspTime = AudioSettings.dspTime;
        songPosition = ((float)(dspTime - songStartDSPTime) + songOffset) % beatmap.songLength;
        songPositionInBeats = songPosition / secondsPerBeat;

        int currentBeatIndex = Mathf.FloorToInt(songPositionInBeats);
        if (currentBeatIndex > lastBeatIndex)
        {
            lastBeatIndex = currentBeatIndex;
            OnBeat?.Invoke();
        }
    }
}
