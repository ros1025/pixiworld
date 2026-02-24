using UnityEngine;

[System.Serializable]
public class MatData
{
    public Color color;
    public Texture2D matPattern;
    public Color patternColor;
    public Texture2D decal;

    public MatData()
    {
        color = Color.gray;
    }

    public MatData(Color baseColor)
    {
        color = baseColor;
    }

    public MatData(MatData clone)
    {
        color = clone.color;
    }
}
