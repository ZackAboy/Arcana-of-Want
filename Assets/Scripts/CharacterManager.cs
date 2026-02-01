using UnityEngine;
using Yarn.Unity;
using System.Collections.Generic;

public class CharacterManager : MonoBehaviour
{
    [Header("Drag all character Image GameObjects here")]
    public List<GameObject> characters;

    private Dictionary<string, GameObject> characterDict;

    void Awake()
    {
        // 初始化字典，并隐藏所有角色
        characterDict = new Dictionary<string, GameObject>();

        foreach (var character in characters)
        {
            if (!characterDict.ContainsKey(character.name))
            {
                characterDict.Add(character.name, character);
                character.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"Duplicate character name: {character.name}");
            }
        }
    }

    // 显示某个角色
    [YarnCommand("show_character")]
    public void ShowCharacter(string name)
    {
        if (characterDict.ContainsKey(name))
        {
            characterDict[name].SetActive(true);
        }
        else
        {
            Debug.LogWarning($"Character not found: {name}");
        }
    }

    // 隐藏某个角色
    [YarnCommand("hide_character")]
    public void HideCharacter(string name)
    {
        if (characterDict.ContainsKey(name))
        {
            characterDict[name].SetActive(false);
        }
        else
        {
            Debug.LogWarning($"Character not found: {name}");
        }
    }

    // 隐藏所有角色（兼容 Yarn 传不传参数）
    [YarnCommand("hide_all_characters")]
    public void HideAllCharacters(params string[] args)
    {
        foreach (var character in characterDict.Values)
        {
            character.SetActive(false);
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
