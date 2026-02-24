using UnityEngine;

[System.Serializable]
public class CharacterItem
{
    public ClothingSO clothingItem;
    //public SkinnedMeshRenderer renderer;
    public MatData matData;

    public CharacterItem(ClothingSO clothingItem, MatData matData)
    {
        this.clothingItem = clothingItem;
        //this.renderer = renderer;
        this.matData = matData;
    }
}