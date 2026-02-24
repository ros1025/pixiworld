using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Collections;

public class CharacterCustomizer : MonoBehaviour
{
    private Character currentCharacter;
    [SerializeField]
    private CharacterManager characterManager;


    private void Awake()
    {
        StartCoroutine(InitializeCustomiser());
    }

    public void AddItemToCharacter(Character character, ClothingSO clothing)
    {
        character.AddItemToCurrentOutfit(clothing);
    }

    public void AddItemToCharacter(Character character, BodyFeatureSO feature)
    {
        character.AddItemToCurrentOutfit(feature);
    }

    public IEnumerator InitializeCustomiser()
    {
        yield return new WaitUntil(() => CharacterManager.instance != null);

        if (currentCharacter == null)
        {
            if (characterManager.GetCharactersCount() == 0)
            {
                characterManager.AddNewCharacter(characterManager.characterRules.defaultAgeGroup);
            }

            currentCharacter = characterManager.GetLastCharacter();
        }
    }

    public void ChangeCurrentChar(int index)
    {
        Character newCharacter = characterManager.GetCharacter(index);

        if (newCharacter != null)
        {
            currentCharacter.HideCharacter();
            newCharacter.ShowCharacter();

            currentCharacter = newCharacter;
        }
    }

    public List<TransformGroups> GetTransformGroups()
    {
        return currentCharacter.shapeKeys;
    }

    public void ChangeCharacterClothing(ClothingSO clothing)
    {
        AddItemToCharacter(currentCharacter, clothing);
    }

    public void ChangeCharacterClothing(BodyFeatureSO bodyFeature)
    {
        AddItemToCharacter(currentCharacter, bodyFeature);
    }

    public List<Character> GetCharacters()
    {
        return characterManager.characters;
    }

    public Character GetCurrentCharacter()
    {
        return currentCharacter;
    }

    public void AddNewCharacter()
    {
        characterManager.AddNewCharacter(currentCharacter.ageGroup);
        currentCharacter = characterManager.GetLastCharacter();
    }
}
