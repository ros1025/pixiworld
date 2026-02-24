using UnityEngine;

[CreateAssetMenu(fileName = "AgeGroupSO", menuName = "Scriptable Objects/AgeGroupSO")]
public class AgeGroupSO : ScriptableObject
{
    [field: SerializeField]
    public string Name {get; private set;}
    [field: SerializeField]
    public AgeGroupMeshSO CharacterObjectReference {get; private set;}
}