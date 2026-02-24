using UnityEngine;

[CreateAssetMenu(fileName = "PersonalityTraitSO", menuName = "Scriptable Objects/PersonalityTraitSO")]
public class PersonalityTraitSO : ScriptableObject
{
    public string Name {get; private set;}
}
