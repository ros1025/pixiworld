using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ObjectData", menuName = "Scriptable Objects/ObjectData")]
public class ObjectData : ScriptableObject
{
    [field: SerializeField]
    public string Name { get; private set; }
    [field: SerializeField]
    public int ID { get; private set; }
    [field: SerializeField]
    public int Cost { get; private set; }
    [field: SerializeField]
    public Vector2Int Size { get; private set; } = Vector2Int.one;
    [field: SerializeField]
    public GameObject Prefab { get; private set; }
    [field: SerializeField]
    public List<ObjectCategory> objectTypeId { get; private set; }
}