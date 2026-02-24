using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OutfitTypeSO", menuName = "Scriptable Objects/OutfitTypeSO")]
public class OutfitTypeSO : ScriptableObject
{
    [field: SerializeField]
    public string Name {get; private set;}

    public List<OutfitTypeRestrictions> defaultRestrictions;
}

[Serializable]
public class OutfitTypeRestrictions
{
    public List<ClothingSO> defaultItems;
    public ClothingPreference clothingGender;
    public AgeGroupMeshSO ageGroup;
}
