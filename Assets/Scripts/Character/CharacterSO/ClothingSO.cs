using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ClothingSO", menuName = "Scriptable Objects/ClothingSO")]
public class ClothingSO : ScriptableObject
{
    [field: SerializeField]
    public string Name { get; private set; }
    [field: SerializeField]
    public int ID {get; private set;}

    [field: SerializeField]
    public Mesh mesh { get; private set; }
    [field: SerializeField]
    public ClothingPreference clothingPreference { get; private set; }
    [field: SerializeField]
    public AgeGroupMeshSO ageGroup {get; private set;}

    [field: SerializeField]
    public List<ObjectCategory> clothingCategory { get; private set; }
}
