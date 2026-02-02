using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

[System.Serializable]
public class NamedSprite
{
    public string key;
    public Sprite sprite;
}

public class BackgroundManager : MonoBehaviour
{
    [Header("UI Image that shows the background (usually on the Canvas)")]
    public Image backgroundImage;

    [Header("Keyed sprites you can reference from Yarn (key -> sprite)")]
    public List<NamedSprite> backgrounds = new List<NamedSprite>();

    [Header("Crossfade settings")]
    public bool useCrossfade = true;
    public float fadeDuration = 0.5f;

    [Header("Optional: DialogueRunner to auto-register command handler")]
    public DialogueRunner dialogueRunner;

    private void Awake()
    {
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
            if (backgroundImage == null)
                backgroundImage = GetComponentInChildren<Image>(true);
        }

        if (dialogueRunner == null)
        {
#if UNITY_2023_1_OR_NEWER
            dialogueRunner = Object.FindFirstObjectByType<DialogueRunner>();
#else
            dialogueRunner = Object.FindObjectOfType<DialogueRunner>();
#endif
        }

        // NOTE: We use the [YarnCommand("set_background")] attribute on SetBackground,
        // which registers the command with Yarn Spinner via reflection. Do not also
        // call AddCommandHandler here or you'll register the same command twice.
    }

    // Yarn command usage: <<set_background key>>  (use quotes if key contains spaces)
    // We register the command manually to avoid conflicts where Yarn treats the
    // first argument as a GameObject target. This prevents the runtime error
    // complaining that "Street doesn't have the correct component".
    private static DialogueRunner registeredRunner = null;

    private void RegisterCommand()
    {
        if (dialogueRunner == null)
            return;

        if (registeredRunner == dialogueRunner)
            return;

        dialogueRunner.AddCommandHandler("set_background", (System.Action<string>)SetBackground);
        registeredRunner = dialogueRunner;
        Debug.Log($"BackgroundManager: registered Yarn command 'set_background' on runner '{dialogueRunner.name}'");
    }

    private void OnEnable()
    {
        RegisterCommand();
    }

    public void SetBackground(string key)
    {
        Debug.Log($"BackgroundManager: SetBackground called with raw key: {key}");
        Debug.Log($"BackgroundManager: backgroundImage assigned? {backgroundImage != null}");
        StartCoroutine(SetBackgroundCoroutine(key));
    }

    private IEnumerator SetBackgroundCoroutine(string rawKey)
    {
        if (backgroundImage == null)
        {
            Debug.LogWarning("BackgroundManager: no Background Image assigned or found.");
            yield break;
        }

        var key = rawKey.Trim().Trim('"');
        var sprite = FindSprite(key);
        if (sprite == null)
        {
            Debug.LogWarning($"BackgroundManager: sprite not found for key '{key}'");
            yield break;
        }

        if (!useCrossfade)
        {
            backgroundImage.sprite = sprite;
            yield break;
        }

        // Crossfade: fade alpha to 0, swap sprite, fade back to 1
        backgroundImage.canvasRenderer.SetAlpha(1f);
        backgroundImage.CrossFadeAlpha(0f, fadeDuration, false);
        yield return new WaitForSecondsRealtime(fadeDuration);

        backgroundImage.sprite = sprite;
        backgroundImage.canvasRenderer.SetAlpha(0f);
        backgroundImage.CrossFadeAlpha(1f, fadeDuration, false);
    }

    private Sprite FindSprite(string key)
    {
        if (string.IsNullOrEmpty(key))
            return null;

        foreach (var ns in backgrounds)
        {
            if (ns == null)
                continue;

            if (string.Equals(ns.key, key, System.StringComparison.OrdinalIgnoreCase))
                return ns.sprite;
        }

        return null;
    }
}
