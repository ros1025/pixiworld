using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AgeGroupsSO", menuName = "Scriptable Objects/AgeGroupsSO")]
public class AgeGroupsSO : ScriptableObject
{
    [field: SerializeField]
    public List<AgeGroupSO> ageGroups {get; private set;}
}
