using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectCategory", menuName = "Scriptable Objects/ObjectCategory")]
public class ObjectCategory : ScriptableObject
{
    [field: SerializeField]
    public string Name { get; private set; }
    [field: SerializeField]
    public Texture2D icon {get; private set; }
}
