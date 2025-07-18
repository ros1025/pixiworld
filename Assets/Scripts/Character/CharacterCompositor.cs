using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem.XR;

public class CharacterCompositor : MonoBehaviour
{
    public List<CharacterItem> items;
    public SkinnedMeshRenderer body;

    [SerializeField]
    private Transform rootBone;

    [SerializeField]
    private Material clothingMaterial;


    public void AddItem(ClothingSO clothing, ClothingCategoryDatabaseSO categories)
    {
        GameObject newItem = new();
        SkinnedMeshRenderer meshRenderer = newItem.AddComponent<SkinnedMeshRenderer>();

        //Update mesh
        meshRenderer.sharedMesh = clothing.mesh;

        //Update bounding box
        meshRenderer.ResetBounds();

        //Update bone structure so that the mesh can be rendered properly
        meshRenderer.rootBone = body.rootBone;
        meshRenderer.bones = body.bones;

        Material mat = Instantiate(clothingMaterial);
        meshRenderer.sharedMaterial = mat;

        meshRenderer.transform.parent = this.transform.parent;

        List<CharacterItem> removeItems = new();
        foreach (CharacterItem item in items)
        {
            if (categories.clothingCategories.FindIndex(
                findItem => findItem.subcategories.Intersect(item.clothingItem.clothingCategory).Count() > 0 &&
            findItem.subcategories.Intersect(clothing.clothingCategory).Count() > 0) != -1)
            {
                removeItems.Add(item);
            }
        }

        CharacterItem savedItem = new CharacterItem(clothing, meshRenderer);


        foreach (CharacterItem item in removeItems)
        {
            GameObject obj = item.renderer.gameObject;

            items.Remove(item);
            Destroy(obj);
        }

        items.Add(savedItem);
    }

    private void GetRecursiveBones(List<Transform> bones, Transform currentBone)
    {
        bones.Add(currentBone);

        for (int i = 0; i < currentBone.childCount; i++)
        {
            GetRecursiveBones(bones, currentBone.GetChild(i));
        }
    }
}
