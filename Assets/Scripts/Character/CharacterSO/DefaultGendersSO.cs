using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultGendersSO", menuName = "Scriptable Objects/DefaultGendersSO")]
public class DefaultGendersSO : ScriptableObject
{
    [field: SerializeField]
    public List<GenderSettingSO> defaultGenders {get; private set;}
}
