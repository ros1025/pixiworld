using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ClothingDatabaseSO", menuName = "Scriptable Objects/ClothingDatabaseSO")]
public class ClothingDatabaseSO : ScriptableObject
{
    public List<ClothingSO> clothes;
}
