using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class BeatConductor : MonoBehaviour
{
    public static BeatConductor Instance;
    public static event System.Action OnBeat;

    [Header("References")]
    public BeatmapData beatmap;
    [EventRef] public string musicEvent;

    [Header("Song Timing")]
    public float songPosition;
    public float songPositionInBeats;

    private EventInstance _eventInstance;
    private float secondsPerBeat;
    private int lastBeatIndex = -1;

    [Header("Offsets")]
    public float songOffset = 0f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        secondsPerBeat = 60f / beatmap.bpm;
        beatmap.GenerateBeats();
        StartSong();
    }

    void StartSong()
    {
        _eventInstance = RuntimeManager.CreateInstance(musicEvent);
        _eventInstance.start();
    }

    void Update()
    {
        _eventInstance.getTimelinePosition(out int posMs);
        songPosition = posMs / 1000f + songOffset;
        songPositionInBeats = songPosition / secondsPerBeat;

        int currentBeatIndex = Mathf.FloorToInt(songPositionInBeats);
        if (currentBeatIndex < lastBeatIndex)
            lastBeatIndex = -1;
        if (currentBeatIndex > lastBeatIndex)
        {
            lastBeatIndex = currentBeatIndex;
            OnBeat?.Invoke();
        }
    }

    public void SetParameter(string name, float value)
    {
        _eventInstance.setParameterByName(name, value);
    }

    void OnDestroy()
    {
        _eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _eventInstance.release();
    }
}
