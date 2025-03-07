using UnityEngine;
using System.Collections.Generic;

public class CharacterCustomizer : MonoBehaviour
{
    [SerializeField]
    private Character currentCharacter;
    [SerializeField]
    private List<Character> characters;

    public void InitializeCustomiser()
    {
        List<TransformGroups> transformGroups = GetTransformGroups();
        foreach (TransformGroups group in transformGroups)
        {
            group.SetDefaultPos();
        }
    }

    public List<TransformGroups> GetTransformGroups()
    {
        return currentCharacter.attributes.transformGroups;
    }
}
