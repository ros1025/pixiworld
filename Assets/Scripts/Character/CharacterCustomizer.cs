using UnityEngine;
using System.Collections.Generic;

public class CharacterCustomizer : MonoBehaviour
{
    [SerializeField]
    private Character currentCharacter;
    [SerializeField]
    private List<Character> characters;
    [SerializeField]
    private ClothingCategoryDatabaseSO categories;

    public void InitializeCustomiser()
    {
        List<TransformGroups> transformGroups = GetTransformGroups();
    }

    public List<TransformGroups> GetTransformGroups()
    {
        return currentCharacter.GetBodyShapeKeys();
    }

    public void ChangeCharacterClothing(ClothingSO clothing)
    {
        currentCharacter.ChangeClothing(clothing, categories);
    }
}
