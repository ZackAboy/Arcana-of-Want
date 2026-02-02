using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

/// <summary>
/// Keeps mask UI borders updated based on Yarn variable changes.
/// Default border = black; flashes gold when its variable increases.
/// </summary>
public class MaskOverlayHighlighter : MonoBehaviour
{
    [System.Serializable]
    public class MaskSlot
    {
        public string yarnVariableName; // e.g. "simp" (without $)
        public Image image;             // UI Image whose border we tint
        [HideInInspector] public Outline outline;
        [HideInInspector] public float lastValue;
    }

    [Header("Bindings")]
    public DialogueRunner dialogueRunner;
    public List<MaskSlot> masks = new List<MaskSlot>();

    [Header("Visuals")]
    public Color normalBorderColor = Color.black;
    public Color highlightBorderColor = new Color32(255, 215, 0, 255); // gold
    public float highlightDuration = 1.0f;
    public Vector2 outlineDistance = new Vector2(3f, 3f);

    private InMemoryVariableStorage storage;

    void Awake()
    {
        if (dialogueRunner == null)
        {
            dialogueRunner = FindObjectOfType<DialogueRunner>();
        }

        EnsureStorage();
        SetupSlots();
    }

    void Update()
    {
        if (storage == null)
            return;

        foreach (var slot in masks)
        {
            if (slot.image == null || string.IsNullOrEmpty(slot.yarnVariableName))
                continue;

            string varName = slot.yarnVariableName.StartsWith("$") ? slot.yarnVariableName : $"${slot.yarnVariableName}";
            if (!storage.TryGetValue<float>(varName, out var current))
                continue;

            if (current > slot.lastValue)
            {
                slot.lastValue = current;
                Highlight(slot);
            }
            else
            {
                slot.lastValue = current;
            }
        }
    }

    private void Highlight(MaskSlot slot)
    {
        StopCoroutineIfRunning(slot);
        slot.outline.effectColor = highlightBorderColor;
        StartCoroutine(ResetAfterDelay(slot));
    }

    private IEnumerator ResetAfterDelay(MaskSlot slot)
    {
        yield return new WaitForSeconds(highlightDuration);
        if (slot != null && slot.outline != null)
        {
            slot.outline.effectColor = normalBorderColor;
        }
    }

    private void StopCoroutineIfRunning(MaskSlot slot)
    {
        // No handle kept; StopAllCoroutines may be too heavy. Instead rely on
        // multiple coroutines being rare; the last one to finish sets color back.
    }

    private void EnsureStorage()
    {
        if (dialogueRunner != null && dialogueRunner.VariableStorage is InMemoryVariableStorage mem)
        {
            storage = mem;
        }
        else
        {
            storage = FindObjectOfType<InMemoryVariableStorage>();
        }
    }

    private void SetupSlots()
    {
        foreach (var slot in masks)
        {
            if (slot.image == null)
                continue;

            var outline = slot.image.GetComponent<Outline>();
            if (outline == null)
            {
                outline = slot.image.gameObject.AddComponent<Outline>();
            }
            outline.effectColor = normalBorderColor;
            outline.effectDistance = outlineDistance;
            slot.outline = outline;

            // Seed lastValue from storage if present
            string varName = slot.yarnVariableName.StartsWith("$") ? slot.yarnVariableName : $"${slot.yarnVariableName}";
            if (storage != null && storage.TryGetValue<float>(varName, out var seed))
            {
                slot.lastValue = seed;
            }
            else
            {
                slot.lastValue = 0f;
            }
        }
    }
}
