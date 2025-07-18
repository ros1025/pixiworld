using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class TransformGroups
{
    public string name;
    public int index;
    public float weight;
    public Character character;

    public TransformGroups(string name, int index, float weight, Character character)
    {
        this.name = name;
        this.index = index;
        this.weight = weight;
        this.character = character;
    }

    public void SetDefaultPos()
    {
        weight = 0;
    }

    public void SetTransformerWeights(float weight)
    {
        this.weight = weight;
        AdjustWeights();
    }

    public void AdjustWeights()
    {
        character.attributes.body.SetBlendShapeWeight(index, weight);

        foreach (CharacterItem additionalItems in character.attributes.items)
        {
            int customIndex = additionalItems.renderer.sharedMesh.GetBlendShapeIndex(name);
            if (customIndex != -1)
            {
                additionalItems.renderer.SetBlendShapeWeight(customIndex, weight);
            }
        }
    }
}
