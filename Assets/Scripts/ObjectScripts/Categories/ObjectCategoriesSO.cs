using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ObjectCategoriesSO", menuName = "Scriptable Objects/ObjectCategoriesSO")]
public class ObjectCategoriesSO : ScriptableObject
{
    public List<ObjectCategory> categories;
}


