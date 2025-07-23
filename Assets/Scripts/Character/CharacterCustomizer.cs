using UnityEngine;
using System.Collections.Generic;

public class CharacterCustomizer : MonoBehaviour
{
    [SerializeField]
    private Character currentCharacter;
    [SerializeField]
    private CharacterManager characterManager;
    [SerializeField]
    private ClothingCategoryDatabaseSO categories;

    void Awake()
    {
        InitializeCustomiser();
    }

    public void InitializeCustomiser()
    {
        if (currentCharacter == null)
        {
            if (characterManager.GetCharactersCount() == 0)
            {
                characterManager.AddNewCharacter();
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
        return currentCharacter.GetBodyShapeKeys();
    }

    public void ChangeCharacterClothing(ClothingSO clothing)
    {
        currentCharacter.ChangeClothing(clothing, categories);
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
        characterManager.AddNewCharacter();
        currentCharacter = characterManager.GetLastCharacter();
    }
}
