using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class TransformGroups
{
    public string name;
    public List<BoneTransformer> bones;
    public float weightX;
    public float weightY;
    public float weightZ;

    public TransformGroups(string name, List<BoneTransformer> bones)
    {
        this.name = name;
        this.bones = bones;
        weightX = 0;
        weightY = 0;
        weightZ = 0;
    }

    public void SetDefaultPos()
    {
        for (int i = 0; i < bones.Count; i++)
        {
            bones[i].SetDefaultPos();
        }
    }

    public void SetTransformerWeights(float weightX, float weightY, float weightZ)
    {
        this.weightX = weightX;
        this.weightY = weightY;
        this.weightZ = weightZ;

        AdjustWeights();
    }

    public void AdjustWeights()
    {
        foreach (BoneTransformer bone in bones)
        {
            if (bone.isReverse)
            {
                bone.bone.position = bone.GetDefaultPos() + new Vector3(-weightX, weightY, weightZ);
            }
            else
            {
                bone.bone.position = bone.GetDefaultPos() + new Vector3(weightX, weightY, weightZ);
            }
        }
    }
}

[Serializable]
public class BoneTransformer
{
    public Transform bone;
    public bool isReverse;
    private Vector3 defaultPos = new();

    public void SetDefaultPos()
    {
        defaultPos = bone.position;
    }

    public Vector3 GetDefaultPos()
    {
        return defaultPos;
    }
}
