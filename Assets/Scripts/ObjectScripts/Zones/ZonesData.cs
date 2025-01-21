using UnityEngine;

[CreateAssetMenu(fileName = "ZonesData", menuName = "Scriptable Objects/ZonesData")]
public class ZonesData : ScriptableObject
{
    [field: SerializeField]
    public string Name { get; private set; }
    [field: SerializeField]
    public int ID { get; private set; }
    //[field: SerializeField]
    //public Zone zoneType { get; private set; }
    [field: SerializeField]
    public int Cost { get; private set; }
}
