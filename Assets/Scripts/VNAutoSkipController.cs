using UnityEngine;
using Yarn.Unity;


public class VNAutoSkipController : MonoBehaviour
{
    [Header("Yarn References")]
    public LineAdvancer lineAdvancer;
    public DialogueRunner dialogueRunner;

    [Header("Modes")]
    public bool autoMode;
    public bool skipMode;

    [Header("Auto Settings")]
    public float autoDelay = 0.6f;

    float autoTimer;
    float skipTimer;
    const float skipInterval = 0.03f;

    bool autoLockedAfterChoice = false;


    void Awake()
    {
        // IMPORTANT: listen for choices
        dialogueRunner.onDialogueStart.AddListener(OnDialogueStarted);
        dialogueRunner.onDialogueComplete.AddListener(StopAllModes);
    }

    void Update()
    {
        if (!dialogueRunner.IsDialogueRunning)
            return;

        // SKIP MODE
        if (skipMode)
        {
            skipTimer += Time.deltaTime;
            if (skipTimer >= skipInterval)
            {
                skipTimer = 0f;

                // Skip = hurry + advance
                lineAdvancer.RequestLineHurryUp();
                lineAdvancer.RequestNextLine();
            }
            return;
        }

        // AUTO MODE (only if not locked by a choice)
        if (autoMode && !autoLockedAfterChoice)
        {
            autoTimer += Time.deltaTime;
            if (autoTimer >= autoDelay)
            {
                autoTimer = 0f;
                lineAdvancer.RequestNextLine();
            }
        }

    }

    // ---------- BUTTONS ----------

    public void ToggleAuto()
    {
        autoMode = !autoMode;
        if (autoMode)
            skipMode = false;

        autoTimer = 0f;
    }

    public void ToggleSkip()
    {
        skipMode = !skipMode;
        if (skipMode)
            autoMode = false;

        skipTimer = 0f;
    }

    // ---------- STOP MODES ----------

    public void StopAllModes()
    {
        autoMode = false;
        skipMode = false;
        autoTimer = 0f;
        skipTimer = 0f;
    }

    // Called automatically when dialogue starts
    void OnDialogueStarted()
    {
        StopAllModes();
    }

}
