using UnityEngine;

[System.Serializable]
public class CharacterBodyFeature
{
    public BodyFeatureSO bodyFeature;
    public MatData matData;

    public CharacterBodyFeature(BodyFeatureSO bodyFeature, MatData matData)
    {
        this.bodyFeature = bodyFeature;
        this.matData = matData;
    }
}
