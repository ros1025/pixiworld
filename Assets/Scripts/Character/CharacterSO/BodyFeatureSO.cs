using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BodyFeatureSO", menuName = "Scriptable Objects/BodyFeatureSO")]
public class BodyFeatureSO : ScriptableObject
{
    [field: SerializeField]
    public string Name { get; private set; }
    [field: SerializeField]
    public int ID {get; private set;}

    [field: SerializeField]
    public Mesh mesh { get; private set; }
    [field: SerializeField]
    public Texture2D skinTexture { get; private set; }
    [field: SerializeField]
    public Texture2D meshTexture { get; private set; }
    [field: SerializeField]
    public ClothingPreference clothingPreference { get; private set; }
    [field: SerializeField]
    public List<ObjectCategory> featureTypes { get; private set; }
}
