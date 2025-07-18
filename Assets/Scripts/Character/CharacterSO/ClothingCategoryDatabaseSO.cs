using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ClothingCategoryDatabaseSO", menuName = "Scriptable Objects/ClothingCategoryDatabaseSO")]
public class ClothingCategoryDatabaseSO : ScriptableObject
{
    [field: SerializeField]
    public List<ClothingCategorySO> clothingCategories { get; private set; }
}