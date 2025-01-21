using UnityEngine;

[CreateAssetMenu(fileName = "DoorsData", menuName = "Scriptable Objects/DoorsData")]
public class DoorsData : ScriptableObject
{
    [field: SerializeField]
    public string Name { get; private set; }
    [field: SerializeField]
    public int ID { get; private set; }
    [field: SerializeField]
    public int Cost { get; private set; }
    [field: SerializeField]
    public int Length { get; private set; } = 1;
    [field: SerializeField]
    public GameObject Prefab { get; private set; }
}
