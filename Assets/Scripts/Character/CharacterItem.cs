using UnityEngine;

[System.Serializable]
public class CharacterItem
{
    public ClothingSO clothingItem;
    public SkinnedMeshRenderer renderer;

    public CharacterItem(ClothingSO clothingItem, SkinnedMeshRenderer renderer)
    {
        this.clothingItem = clothingItem;
        this.renderer = renderer;
    }
}
