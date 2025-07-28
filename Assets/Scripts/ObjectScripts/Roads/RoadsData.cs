using UnityEngine;

[CreateAssetMenu(fileName = "RoadsData", menuName = "Scriptable Objects/RoadsData")]
public class RoadsData : ScriptableObject
{
    [field: SerializeField]
    public string Name { get; private set; }
    [field: SerializeField]
    public long ID { get; private set; }
    [field: SerializeField]
    public int Cost { get; private set; }
    [field: SerializeField]
    public int width { get; private set; } = 1;
    [field: SerializeField]
    public bool TwoWay { get; private set; }
    [field: SerializeField]
    public Texture2D tex { get; private set; }
}
