using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OutfitTypesSO", menuName = "Scriptable Objects/OutfitTypesSO")]
public class OutfitTypesSO : ScriptableObject
{
    [field: SerializeField]
    public List<OutfitTypeSO> outfitTypes {get; private set;}
}
