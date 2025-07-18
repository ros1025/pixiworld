using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ClothingCategorySO", menuName = "Scriptable Objects/ClothingCategorySO")]
public class ClothingCategorySO : ScriptableObject
{
    [field: SerializeField]
    public string name { get; private set; }

    [field: SerializeField]
    public List<ObjectCategory> subcategories { get; private set; }
}
