using UnityEngine;

[CreateAssetMenu(fileName = "CharacterRulesSO", menuName = "Scriptable Objects/CharacterRulesSO")]
public class CharacterRulesSO : ScriptableObject
{
    [field: SerializeField]
    public ClothingCategoryDatabaseSO clothingCategories {get; private set;}
    [field: SerializeField]
    public ClothingCategoryDatabaseSO bodyFeatureCategories {get; private set;}
    [field: SerializeField]
    public OutfitTypesSO outfitTypes {get; private set;}
    [field: SerializeField]
    public AgeGroupsSO ageGroups {get; private set;}
    [field: SerializeField]
    public AgeGroupSO defaultAgeGroup {get; private set;}
    [field: SerializeField]
    public DefaultGendersSO defaultGenders {get; private set;}

    [field: SerializeField]
    public Material clothingMaterial {get; private set;}
    [field: SerializeField]
    public Material hairMaterial {get; private set;}
}
