using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ClothingSO", menuName = "Scriptable Objects/ClothingSO")]
public class ClothingSO : ScriptableObject
{
    [field: SerializeField]
    public string name { get; private set; }

    [field: SerializeField]
    public Mesh mesh { get; private set; }

    [field: SerializeField]
    public List<ObjectCategory> clothingCategory { get; private set; }
}
