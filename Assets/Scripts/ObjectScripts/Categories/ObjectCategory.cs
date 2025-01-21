using UnityEngine;

[CreateAssetMenu(fileName = "ObjectCategory", menuName = "Scriptable Objects/ObjectCategory")]
public class ObjectCategory : ScriptableObject
{
    [field: SerializeField]
    public string Name { get; private set; }
}
