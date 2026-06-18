using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance;

    [Header("References")]
    [SerializeField] GameObject panel;
    [SerializeField] TMP_Text speakerNameText;
    [SerializeField] bool showSpeakerName = false;
    [SerializeField] TMP_Text lineText;

    [Header("Typing")]
    [SerializeField] float charsPerSecond = 40f;

    string[] pendingLines;
    int lineIndex;
    bool isTyping;
    bool inputBlocked;
    Coroutine typeCoroutine;
    Action pendingOnComplete;
    Action pendingOnLastLineAdvanced;

    void Awake()
    {
        Instance = this;
        if (panel != null)
            panel.SetActive(false);
    }

    public void Play(DialogueData data, Action onComplete, Action onLastLineAdvanced = null)
    {
        if (panel == null || lineText == null || data == null || data.lines == null || data.lines.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }

        pendingLines = data.lines;
        pendingOnComplete = onComplete;
        pendingOnLastLineAdvanced = onLastLineAdvanced;
        lineIndex = 0;

        if (speakerNameText != null)
        {
            speakerNameText.gameObject.SetActive(showSpeakerName);
            speakerNameText.text = data.speakerName;
        }

        panel.SetActive(true);
        Time.timeScale = 0f;

        ShowLine(lineIndex);
    }

    void ShowLine(int index)
    {
        if (typeCoroutine != null)
            StopCoroutine(typeCoroutine);
        typeCoroutine = StartCoroutine(TypeLine(pendingLines[index]));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        lineText.text = "";
        float delay = charsPerSecond > 0f ? 1f / charsPerSecond : 0f;

        for (int i = 0; i < line.Length; i++)
        {
            lineText.text += line[i];
            if (delay > 0f)
                yield return new WaitForSecondsRealtime(delay);
        }

        OnLineFullyShown();
    }

    void OnLineFullyShown()
    {
        isTyping = false;
        typeCoroutine = null;

        if (lineIndex == pendingLines.Length - 1)
            BeatConductor.Instance?.SetParameter("Mini Boss Dialogue", 1f);
    }

    public void SetInputBlocked(bool blocked) => inputBlocked = blocked;

    public void AutoClose() => Finish();

    void Update()
    {
        if (panel == null || !panel.activeSelf) return;
        if (inputBlocked) return;
        if (!Input.GetKeyDown(KeyCode.E) && !Input.GetMouseButtonDown(0)) return;

        if (isTyping)
        {
            // First press just snaps the current line to fully revealed.
            StopCoroutine(typeCoroutine);
            lineText.text = pendingLines[lineIndex];
            OnLineFullyShown();
            return;
        }

        Advance();
    }

    void Advance()
    {
        lineIndex++;
        if (lineIndex < pendingLines.Length)
            ShowLine(lineIndex);
        else if (pendingOnLastLineAdvanced != null)
        {
            Action cb = pendingOnLastLineAdvanced;
            pendingOnLastLineAdvanced = null;
            cb.Invoke();
        }
        else
            Finish();
    }

    void Finish()
    {
        inputBlocked = false;
        panel.SetActive(false);
        Time.timeScale = 1f;

        Action callback = pendingOnComplete;
        pendingOnComplete = null;
        pendingOnLastLineAdvanced = null;
        pendingLines = null;
        callback?.Invoke();
    }
}
