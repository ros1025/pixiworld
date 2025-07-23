using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Character : MonoBehaviour
{
    //public GameObject prefab;
    public CharacterCompositor attributes;

    void Awake()
    {
        if (attributes == null || !attributes.transform.IsChildOf(transform))
        {
            this.attributes = this.gameObject.GetComponentInChildren<CharacterCompositor>();
        }
    }

    public List<TransformGroups> GetBodyShapeKeys()
    {
        int index = attributes.body.sharedMesh.blendShapeCount;
        List<TransformGroups> shapeKeys = new();

        for (int i = 0; i < index; i++)
        {
            string name = attributes.body.sharedMesh.GetBlendShapeName(i);
            float weight = attributes.body.GetBlendShapeWeight(i);

            TransformGroups blendShape = new TransformGroups(name, i, weight, this);
            shapeKeys.Add(blendShape);
        }

        return shapeKeys;
    }

    public void ChangeClothing(ClothingSO clothing, ClothingCategoryDatabaseSO categories)
    {
        attributes.AddItem(clothing, categories);
    }

    public void HideCharacter()
    {
        gameObject.SetActive(false);
    }

    public void ShowCharacter()
    {
        gameObject.SetActive(true);
    }
}
