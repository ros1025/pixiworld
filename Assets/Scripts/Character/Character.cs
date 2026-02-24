using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.Analytics;

[Serializable]
public class Character
{
    public GameObject prefab;
    [field: SerializeField]
    public string name {get; private set;}
    public List<SavedCharacterOutfits> outfits;
    public int currentOutfitIndex;
    public CharacterCompositor attributes;
    [field: SerializeField]
    public AgeGroupSO ageGroup {get; private set;}
    [field: SerializeField]
    public GenderSetting gender {get; private set;}
    public List<TransformGroups> shapeKeys {get; private set;}
    private List<GameObject> outfitObjects;

    public Character(AgeGroupSO ageGroup, GenderSetting gender)
    {
        this.prefab = GameObject.Instantiate(ageGroup.CharacterObjectReference.CharacterObject);
        this.name = "Character";
        this.attributes = prefab.GetComponentInChildren<CharacterCompositor>();
        this.ageGroup = ageGroup;
        this.gender = gender;
        this.outfits = new();
        this.shapeKeys = new();
        this.outfitObjects = new();

        GetBodyShapeKeys();

        Debug.Log(CharacterManager.instance);
        foreach (OutfitTypeSO outfitType in CharacterManager.instance.characterRules.outfitTypes.outfitTypes)
        {
            AddOutfit(outfitType);
        }

        SwitchOutfit(outfits[0]);
    }

    private void GetBodyShapeKeys()
    {
        int j = 0;

        for (int i = 0; i < attributes.body.sharedMesh.blendShapeCount; i++)
        {
            string name = attributes.body.sharedMesh.GetBlendShapeName(i);
            float weight = attributes.body.GetBlendShapeWeight(i);
            string[] nameFragments = name.Split("_");

            if (nameFragments[0] == "m")
            {
                TransformGroups blendShape = new TransformGroups(name, j, weight, this);
                shapeKeys.Add(blendShape);
            }
            j++;
        }
    }

    public void AddItemToCurrentOutfit(ClothingSO clothing)
    {
        MatData matData = new MatData(Color.white);

        List<CharacterItem> removeItems = new();
        foreach (CharacterItem item in outfits[currentOutfitIndex].items)
        {
            if (CharacterManager.instance.characterRules.clothingCategories.clothingCategories.FindIndex(
                findItem => findItem.subcategories.Intersect(item.clothingItem.clothingCategory).Count() > 0 &&
            findItem.subcategories.Intersect(clothing.clothingCategory).Count() > 0) != -1)
            {
                removeItems.Add(item);
            }
        }

        CharacterItem savedItem = new CharacterItem(clothing, matData);


        foreach (CharacterItem item in removeItems)
        {
            outfits[currentOutfitIndex].items.Remove(item);
        }

        outfits[currentOutfitIndex].items.Add(savedItem);
        RenderOutfit();
    }

    public void AddItemToCurrentOutfit(BodyFeatureSO feature)
    {
        MatData matData = new MatData(Color.black);

        List<CharacterBodyFeature> removeItems = new();
        foreach (CharacterBodyFeature item in outfits[currentOutfitIndex].features)
        {
            if (CharacterManager.instance.characterRules.bodyFeatureCategories.clothingCategories.FindIndex(
                findItem => findItem.subcategories.Intersect(item.bodyFeature.featureTypes).Count() > 0 &&
            findItem.subcategories.Intersect(feature.featureTypes).Count() > 0) != -1)
            {
                removeItems.Add(item);
            }
        }

        CharacterBodyFeature savedItem = new CharacterBodyFeature(feature, matData);


        foreach (CharacterBodyFeature item in removeItems)
        {
            outfits[currentOutfitIndex].features.Remove(item);
        }

        outfits[currentOutfitIndex].features.Add(savedItem);
        RenderOutfit();
    }

    public void AddItemToOutfit(ClothingSO clothing, SavedCharacterOutfits outfit)
    {
        MatData matData = new MatData(Color.white);

        List<CharacterItem> removeItems = new();
        foreach (CharacterItem item in outfit.items)
        {
            if (CharacterManager.instance.characterRules.clothingCategories.clothingCategories.FindIndex(
                findItem => findItem.subcategories.Intersect(item.clothingItem.clothingCategory).Count() > 0 &&
            findItem.subcategories.Intersect(clothing.clothingCategory).Count() > 0) != -1)
            {
                removeItems.Add(item);
            }
        }

        CharacterItem savedItem = new CharacterItem(clothing, matData);


        foreach (CharacterItem item in removeItems)
        {
            outfit.items.Remove(item);
        }

        outfit.items.Add(savedItem);
    }

    public void AddOutfit(OutfitTypeSO outfitType)
    {
        SavedCharacterOutfits newOutfit = new(outfitType);
        outfits.Add(newOutfit);

        OutfitTypeRestrictions outfitTypeRestrictions = outfitType.defaultRestrictions.Find(item => item.ageGroup == this.ageGroup.CharacterObjectReference && gender.CompatibleClothingPreference(item.clothingGender));
        foreach (ClothingSO clothing in outfitTypeRestrictions.defaultItems)
        {
            AddItemToOutfit(clothing, newOutfit);
        }

        currentOutfitIndex = outfits.IndexOf(newOutfit);
    }

    public void SwitchOutfit(SavedCharacterOutfits outfit)
    {
        if (!outfits.Contains(outfit))
        {
            outfits.Add(outfit);
        }
        
        currentOutfitIndex = outfits.IndexOf(outfit);

        RenderOutfit();
    }

    public List<SavedCharacterOutfits> GetOutfitsByType(OutfitTypeSO outfitType)
    {
        return outfits.FindAll(item => item.outfitType == outfitType);
    }

    public SavedCharacterOutfits GetCurrentOutfit()
    {
        return outfits[currentOutfitIndex];
    }

    public void HideCharacter()
    {
        prefab.SetActive(false);
    }

    public void ShowCharacter()
    {
        prefab.SetActive(true);
    }

    public string GetCharacterName()
    {
        return name;
    }

    public void SetCharacterName(string name)
    {
        this.name = name;
    }

    public GenderSetting GetCharacterGender()
    {
        return gender;
    }

    public void SetCharacterGender(GenderSetting genderSetting)
    {
        if (!(this.gender == genderSetting))
        {
            this.gender = genderSetting;

            RandomizeAllClothing();
        }

        RenderOutfit();
    }

    public AgeGroupSO GetCharacterAge()
    {
        return ageGroup;
    }

    public void SetAgeGroup(AgeGroupSO ageGroup)
    {
        if (!(this.ageGroup == ageGroup))
        {
            this.ageGroup = ageGroup;

            RandomizeAllClothing();
        }

        RenderOutfit();
    }

    public void RandomizeAllClothing()
    {
        foreach (SavedCharacterOutfits outfit in outfits)
        {
            outfit.ClearItems();
            outfit.ClearFeatures();

            OutfitTypeRestrictions outfitTypeRestrictions = outfit.outfitType.defaultRestrictions.Find(item => item.ageGroup == this.ageGroup.CharacterObjectReference && gender.CompatibleClothingPreference(item.clothingGender));
            foreach (ClothingSO clothing in outfitTypeRestrictions.defaultItems)
            {
                AddItemToOutfit(clothing, outfit);
            }
        }
    }

    public void RenderOutfit()
    {
        foreach (GameObject gameObject in outfitObjects)
        {
            GameObject.Destroy(gameObject);
        }
        outfitObjects.Clear();

        foreach (CharacterItem item in outfits[currentOutfitIndex].items)
        {
            GameObject newItem = new();
            SkinnedMeshRenderer renderer = newItem.AddComponent<SkinnedMeshRenderer>();
            renderer.sharedMesh = item.clothingItem.mesh;

            //Update bounding box
            renderer.ResetBounds();

            //Update bone structure so that the mesh can be rendered properly
            renderer.rootBone = attributes.body.rootBone;
            renderer.bones = attributes.body.bones;

            Material material = GameObject.Instantiate(CharacterManager.instance.characterRules.clothingMaterial);
            material.color = item.matData.color;
            material.SetTexture("_Pattern", item.matData.matPattern);
            material.SetColor("_PatternColor", item.matData.patternColor);
            material.SetTexture("_Decal", item.matData.decal);
            renderer.sharedMaterial = material;

            newItem.transform.parent = prefab.transform;
            outfitObjects.Add(newItem);
        }

        foreach (CharacterBodyFeature feature in outfits[currentOutfitIndex].features)
        {
            GameObject newItem = new();
            SkinnedMeshRenderer renderer = newItem.AddComponent<SkinnedMeshRenderer>();
            renderer.sharedMesh = feature.bodyFeature.mesh;

            //Update bounding box
            renderer.ResetBounds();

            //Update bone structure so that the mesh can be rendered properly
            renderer.rootBone = attributes.body.rootBone;
            renderer.bones = attributes.body.bones;

            Material material = GameObject.Instantiate(CharacterManager.instance.characterRules.hairMaterial);
            material.color = feature.matData.color;
            material.SetTexture("_Pattern", feature.bodyFeature.meshTexture);
            material.SetColor("_PatternColor", Color.white);
            renderer.sharedMaterial = material;

            newItem.transform.parent = prefab.transform;
            outfitObjects.Add(newItem);
        }

        SetAllWeightsForFeatures();
    }

    public void SetWeightForFeatures(TransformGroups transformGroup)
    {
        foreach (GameObject outfitObject in outfitObjects)
        {
            SkinnedMeshRenderer renderer = outfitObject.GetComponent<SkinnedMeshRenderer>();
            int customIndex = renderer.sharedMesh.GetBlendShapeIndex(transformGroup.name);
            if (customIndex != -1)
            {
                renderer.SetBlendShapeWeight(customIndex, transformGroup.weight);
            }
        }
    }

    public void SetAllWeightsForFeatures()
    {
        foreach (TransformGroups transformGroup in shapeKeys)
        {
            foreach (GameObject outfitObject in outfitObjects)
            {
                SkinnedMeshRenderer renderer = outfitObject.GetComponent<SkinnedMeshRenderer>();
                int customIndex = renderer.sharedMesh.GetBlendShapeIndex(transformGroup.name);
                if (customIndex != -1)
                {
                    renderer.SetBlendShapeWeight(customIndex, transformGroup.weight);
                }
            }
        }
    }

    public List<CharacterItem> GetAllClothingInOutfit(int outfitIndex)
    {
        List<CharacterItem> items = new();

        foreach (CharacterItem item in outfits[outfitIndex].items)
        {
            items.Add(item);
        }

        return items;
    }

    public List<CharacterItem> GetAllClothingInCurrentOutfit()
    {
        List<CharacterItem> items = new();

        foreach (CharacterItem item in outfits[currentOutfitIndex].items)
        {
            items.Add(item);
        }

        return items;
    }
}
