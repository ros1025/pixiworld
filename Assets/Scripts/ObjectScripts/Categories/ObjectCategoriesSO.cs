using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ObjectCategoriesSO", menuName = "Scriptable Objects/ObjectCategoriesSO")]
public class ObjectCategoriesSO : ScriptableObject
{
    public List<ObjectCategory> categories;
}

[CreateAssetMenu(fileName = "ObjectCategory", menuName = "Scriptable Objects/ObjectCategory")]
public class ObjectCategory : ScriptableObject
{
    [field:SerializeField]
    public string Name { get; private set; }
}
