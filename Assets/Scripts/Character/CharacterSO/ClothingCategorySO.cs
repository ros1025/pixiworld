using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ClothingCategorySO", menuName = "Scriptable Objects/ClothingCategorySO")]
public class ClothingCategorySO : ScriptableObject
{
    public List<ClothingCategory> clothingCategories;
}

[Serializable]
public class ClothingCategory
{
    public string name;
    public List<ObjectCategory> subcategories;
}
