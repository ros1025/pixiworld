using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SavedCharacterOutfits
{
    public List<CharacterItem> items;
    public List<CharacterBodyFeature> features;
    public OutfitTypeSO outfitType;

    public SavedCharacterOutfits(OutfitTypeSO outfitTypeSO)
    {
        this.outfitType = outfitTypeSO;

        items = new();
        features = new();
    }

    public void ClearItems()
    {
        items.Clear();
    }

    public void ClearFeatures()
    {
        features.Clear();
    }
}
