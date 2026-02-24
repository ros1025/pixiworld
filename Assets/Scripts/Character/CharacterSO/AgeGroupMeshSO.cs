using UnityEngine;

[CreateAssetMenu(fileName = "AgeGroupMeshSO", menuName = "Scriptable Objects/AgeGroupMeshSO")]
public class AgeGroupMeshSO : ScriptableObject
{
    [field: SerializeField]
    public GameObject CharacterObject {get; private set;}
}
