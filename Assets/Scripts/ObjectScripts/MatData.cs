using UnityEngine;

[System.Serializable]
public class MatData
{
    public Color color;

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
