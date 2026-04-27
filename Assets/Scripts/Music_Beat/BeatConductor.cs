using UnityEngine;
using System;
using System.Runtime.InteropServices;
using FMODUnity;
using FMOD.Studio;

public class BeatConductor : MonoBehaviour
{
    public static BeatConductor Instance;
    public static event System.Action OnBeat;

    [Header("References")]
    [EventRef] public string musicEvent;

    [Header("Song Timing")]
    public float songPosition;
    public float songPositionInBeats;
    public float secondsPerBeat;
    public float lastBeatTime;

    [Header("Offsets")]
    public float songOffset = 0f;

    private EventInstance _eventInstance;

    // Kept as a field to prevent garbage collection
    private static readonly EVENT_CALLBACK _beatCallback = new EVENT_CALLBACK(BeatEventCallback);

    private volatile bool _beatPending;
    private volatile int _pendingBeatPositionMs;
    private volatile float _pendingTempo;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _eventInstance = RuntimeManager.CreateInstance(musicEvent);
        _eventInstance.setCallback(_beatCallback, EVENT_CALLBACK_TYPE.TIMELINE_BEAT);
        _eventInstance.start();
    }

    void Update()
    {
        _eventInstance.getTimelinePosition(out int posMs);
        songPosition = posMs / 1000f + songOffset;

        if (secondsPerBeat > 0)
            songPositionInBeats = songPosition / secondsPerBeat;

        if (_beatPending)
        {
            _beatPending = false;
            lastBeatTime = _pendingBeatPositionMs / 1000f + songOffset;
            secondsPerBeat = 60f / _pendingTempo;
            OnBeat?.Invoke();
        }
    }

    public void SetParameter(string name, float value)
    {
        _eventInstance.setParameterByName(name, value);
    }

    [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
    static FMOD.RESULT BeatEventCallback(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
    {
        if (type == EVENT_CALLBACK_TYPE.TIMELINE_BEAT && Instance != null)
        {
            var props = (TIMELINE_BEAT_PROPERTIES)Marshal.PtrToStructure(
                parameterPtr, typeof(TIMELINE_BEAT_PROPERTIES));
            Instance._pendingBeatPositionMs = props.position;
            Instance._pendingTempo = props.tempo;
            Instance._beatPending = true;
        }
        return FMOD.RESULT.OK;
    }

    void OnDestroy()
    {
        _eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _eventInstance.release();
    }
}
