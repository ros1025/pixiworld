using UnityEngine;

[CreateAssetMenu(fileName = "WindowsData", menuName = "Scriptable Objects/WindowsData")]
public class WindowsData : ScriptableObject
{
    [field: SerializeField]
    public string Name { get; private set; }
    [field: SerializeField]
    public long ID { get; private set; }
    [field: SerializeField]
    public int Cost { get; private set; }
    [field: SerializeField]
    public float Length { get; private set; } = 1;
    [field: SerializeField]
    public float Height { get; private set; } = 1;
    [field: SerializeField]
    public GameObject Prefab { get; private set; }
}
