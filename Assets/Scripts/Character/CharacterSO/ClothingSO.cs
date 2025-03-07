using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ClothingSO", menuName = "Scriptable Objects/ClothingSO")]
public class ClothingSO : ScriptableObject
{
    public GameObject prefab;
    public enum ClothingType
    {
        Open,
        ClosedTop,
        ClosedBottom,
        ClosedFullBody
    }
    public ClothingType type;
    public List<ObjectCategory> clothingCategory;
}
