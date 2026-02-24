using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BodyItemsDatabaseSO", menuName = "Scriptable Objects/BodyItemsDatabaseSO")]
public class BodyItemsDatabaseSO : ScriptableObject
{
    [field: SerializeField]
    public List<BodyFeatureSO> bodyFeatures { get; private set; }

}
