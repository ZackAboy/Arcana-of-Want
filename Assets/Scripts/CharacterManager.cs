using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class CharacterManager : MonoBehaviour
{
    [Header("Drag all character Image GameObjects here")]
    public List<GameObject> characters;

    [Header("Dialogue Runner (optional)")]
    [SerializeField] private DialogueRunner dialogueRunner;

    [Header("Highlight Settings")]
    [SerializeField] private Color highlightedColor = Color.white;
    [SerializeField] private Color dimmedColor = new Color(1f, 1f, 1f, 0.4f);
    [SerializeField] private float highlightScale = 1.05f;
    [SerializeField] private bool dimInactiveCharacters = true;

    private readonly Dictionary<string, GameObject> characterDict = new Dictionary<string, GameObject>(System.StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Image> characterImages = new Dictionary<string, Image>(System.StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Vector3> imageBaseScales = new Dictionary<string, Vector3>(System.StringComparer.OrdinalIgnoreCase);
    private bool handlersRegistered;

    void Awake()
    {
        BuildLookup();
        WireDialogueRunner();
        HideAllCharacters();
    }

    private void OnDestroy()
    {
        UnwireDialogueRunner();
    }

    private void BuildLookup()
    {
        characterDict.Clear();
        characterImages.Clear();

        foreach (var character in characters)
        {
            if (character == null)
            {
                Debug.LogWarning("CharacterManager has a null entry in the characters list.");
                continue;
            }

            var key = character.name;
            if (characterDict.ContainsKey(key))
            {
                Debug.LogWarning($"Duplicate character name: {character.name}");
                continue;
            }

            characterDict.Add(key, character);

            var image = character.GetComponentInChildren<Image>(true);
            if (image != null)
            {
                characterImages[key] = image;
                imageBaseScales[key] = image.rectTransform.localScale;
            }
            else
            {
                Debug.LogWarning($"Character '{character.name}' has no Image component.");
            }

            character.SetActive(false);
        }
    }

    private void WireDialogueRunner()
    {
        if (dialogueRunner == null)
        {
            dialogueRunner = FindRunnerSafe();
        }

        if (dialogueRunner == null)
        {
            Debug.LogWarning("CharacterManager couldn't find a DialogueRunner. Please assign one in the inspector.");
            return;
        }

        if (!handlersRegistered)
        {
            dialogueRunner.AddCommandHandler("show_character", (System.Action<string>)HandleShowCharacterCommand);
            dialogueRunner.AddCommandHandler("hide_character", (System.Action<string>)HandleHideCharacterCommand);
            dialogueRunner.AddCommandHandler("hide_all_characters", (System.Action)HandleHideAllCharactersCommand);
            handlersRegistered = true;
        }

        if (dialogueRunner.onDialogueStart != null)
        {
            dialogueRunner.onDialogueStart.RemoveListener(OnDialogueStarted);
            dialogueRunner.onDialogueStart.AddListener(OnDialogueStarted);
        }

        if (dialogueRunner.onDialogueComplete != null)
        {
            dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);
            dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
        }
    }

    private void UnwireDialogueRunner()
    {
        if (dialogueRunner == null)
            return;

        if (dialogueRunner.onDialogueStart != null)
        {
            dialogueRunner.onDialogueStart.RemoveListener(OnDialogueStarted);
        }

        if (dialogueRunner.onDialogueComplete != null)
        {
            dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);
        }
    }

    private DialogueRunner FindRunnerSafe()
    {
#if UNITY_2023_1_OR_NEWER
        return Object.FindFirstObjectByType<DialogueRunner>();
#else
        return Object.FindObjectOfType<DialogueRunner>();
#endif
    }

    private void OnDialogueStarted() => HideAllCharacters();

    private void OnDialogueComplete() => HideAllCharacters();

    private string CleanName(string rawName)
    {
        return rawName.Trim().Trim('"');
    }

    private void HandleShowCharacterCommand(string name) => ShowCharacter(CleanName(name));

    private void HandleHideCharacterCommand(string name) => HideCharacter(CleanName(name));

    private void HandleHideAllCharactersCommand() => HideAllCharacters();

    // 显示某个角色
    public void ShowCharacter(string name)
    {
        name = CleanName(name);

        if (!characterDict.ContainsKey(name))
        {
            Debug.LogWarning($"Character not found: {name}");
            return;
        }

        foreach (var kvp in characterDict)
        {
            bool isTarget = kvp.Key.Equals(name, System.StringComparison.OrdinalIgnoreCase);
            kvp.Value.SetActive(isTarget || (!dimInactiveCharacters && kvp.Value.activeSelf));
            UpdateVisualState(kvp.Key, isTarget);
        }
    }

    // 隐藏某个角色
    public void HideCharacter(string name)
    {
        name = CleanName(name);

        if (characterDict.ContainsKey(name))
        {
            characterDict[name].SetActive(false);
            UpdateVisualState(name, false);
        }
        else
        {
            Debug.LogWarning($"Character not found: {name}");
        }
    }

    // 隐藏所有角色（兼容 Yarn 传不传参数）
    public void HideAllCharacters()
    {
        foreach (var character in characterDict.Values)
        {
            character.SetActive(false);
            UpdateVisualState(character.name, false);
        }
    }

    private void UpdateVisualState(string characterName, bool highlighted)
    {
        if (!characterImages.TryGetValue(characterName, out var image) || image == null)
        {
            return;
        }

        image.color = highlighted ? highlightedColor : (dimInactiveCharacters ? dimmedColor : highlightedColor);

        if (imageBaseScales.TryGetValue(characterName, out var baseScale))
        {
            image.rectTransform.localScale = baseScale * (highlighted ? highlightScale : 1f);
        }
    }

    // 切换角色：只显示指定角色
    //[YarnCommand("switch_character")]
    //public void SwitchCharacter(string name)
    //{
    //    HideAllCharacters();
    //    ShowCharacter(name);
    //}
}
