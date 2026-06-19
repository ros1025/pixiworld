using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class TransformGroups
{
    public string name;
    public int index;
    public float weight;

    public TransformGroups(string name, int index, float weight)
    {
        this.name = name;
        this.index = index;
        this.weight = weight;
    }

    public void SetDefaultPos()
    {
        weight = 0;
    }

    public void SetTransformerWeights(float weight)
    {
        this.weight = weight;
    }
}
