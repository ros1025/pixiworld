using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ObjectCategoriesSO", menuName = "Scriptable Objects/ObjectCategoriesSO")]
public class ObjectCategoriesSO : ScriptableObject
{
    public List<ObjectCategory> categories;
}

[Serializable]
public class ObjectCategory
{
    [field:SerializeField]
    public string Name { get; private set; }
    [field: SerializeField]
    public int id { get; private set; }
}
