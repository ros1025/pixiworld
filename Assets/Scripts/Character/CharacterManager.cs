using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public List<Character> characters;
    public CharacterRulesSO characterRules;
    public static CharacterManager instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            characters.AddRange(instance.characters);
            Destroy(instance.gameObject);
        }
        instance = this;
    }

    public void AddNewCharacter(AgeGroupSO ageGroup)
    {
        System.Random random = new System.Random();

        //GameObject newCharObject = Instantiate(ageGroup.CharacterObjectReference.CharacterObject);
        GenderSetting gender = characterRules.defaultGenders.defaultGenders[random.Next(0, characterRules.defaultGenders.defaultGenders.Count)].ConvertToGenderSettingObject();
        Character newCharacter = new Character(ageGroup, gender);
        characters.Add(newCharacter);
    }

    public Character GetCharacter(int index)
    {
        Debug.Log($"{index}, {characters.Count}");

        if (index >= 0 && index < characters.Count)
        {
            return characters[index];
        }
        else
        {
            Debug.Log("Invalid character ID!");
            return null;
        }
    }

    public Character GetLastCharacter()
    {
        if (characters.Count == 0)
        {
            AddNewCharacter(characterRules.defaultAgeGroup);
        }

        return characters[^1];
    }

    public int GetCharactersCount()
    {
        return characters.Count;
    }
}
