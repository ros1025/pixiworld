using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacementPreview : IStaticPreviewSystem
{
    private float previewYOffset = 0.05f;

    private Vector2 cursorOffset;

    public GameObject previewObject;

    public GameObject previewSelector;

    private InputManager input;
    private PlacementSystem system;

    private Material previewMaterialInstance;

    private Vector3 previewPos;
    private Vector2Int previewSize;
    public List<MatData> materials;
    


    public void ApplyFeedback(bool validity)
    {
        if (previewObject != null)
        {
            ApplyFeedbackToObject(validity);
        }
        ApplyFeedbackToCursor(validity);
    }

    private void ApplyFeedbackToObject(bool validity)
    {
        Color c = validity ? Color.white : Color.red;

        c.a = 0.5f;
        previewMaterialInstance.color = c;
    }

    private void ApplyFeedbackToCursor(bool validity)
    {
        Color c = validity ? Color.white : Color.red;

        c.a = 0.5f;
        system.cellIndicator.GetComponentInChildren<Renderer>().material.color = c; system.cellIndicator.GetComponentInChildren<Renderer>().material.color = c;
        previewSelector.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = c;
    }

    public bool CheckExpansionHandles()
    {
        return false;
    }

    public bool CheckPreviewObject()
    {
        if (input.RayHitObject(previewSelector.transform.GetChild(0).gameObject))
            return true;
        else
            return false;
    }

    public GameObject GetPreviewObject()
    {
        return previewObject;
    }

    public Vector3 GetPreviewPosition()
    {
        return previewPos;
    }

    public void SetExpansionState(bool state)
    {
        return;
    }

    public void StartPreview(GameObject prefab, Vector2Int size, Vector2 cursorOffset, PlacementSystem system, InputManager input, List<MatData> materials)
    {
        this.system = system;
        this.input = input;
        previewMaterialInstance = new Material(system.previewMaterialPrefab);

        previewObject = GameObject.Instantiate(prefab);
        previewSelector = GameObject.Instantiate(system.previewSelectorObject);
        previewSelector.transform.SetParent(previewObject.transform.transform);
        previewSelector.transform.name = "Selector";
        previewSize = size;

        cursorOffset.x = Math.Abs(cursorOffset.x) < 0.05f ? Math.Sign(cursorOffset.x) == -1 ? -0.05f : 0.05f : cursorOffset.x;
        cursorOffset.y = Math.Abs(cursorOffset.y) < 0.05f ? Math.Sign(cursorOffset.x) == -1 ? -0.05f : 0.05f : cursorOffset.y;
        this.cursorOffset = cursorOffset;

        previewPos = new Vector3(system.cellIndicator.transform.position.x, 0, system.cellIndicator.transform.position.z);
        PrepareCursor(size);
        system.cellIndicator.SetActive(true);

        this.materials = materials;
        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            if (renderer.transform.parent.gameObject != previewSelector)
            {
                for (int j = 0; j < renderer.sharedMaterials.Length; j++)
                {
                    renderer.materials[j] = GameObject.Instantiate(renderer.sharedMaterials[j]);
                    materials.Add(new MatData(renderer.sharedMaterials[j].color));
                }
            }
        }
        MovePreview(system.cellIndicator.transform.position);
    }

    private void PrepareCursor(Vector2Int size)
    {
        if (size.x > 0 || size.y > 0)
        {
            system.cellIndicator.transform.localScale = new Vector3(size.x, size.y, size.y);
            previewSelector.transform.localPosition = new Vector3(cursorOffset.x, 0f, cursorOffset.y);
            previewSelector.transform.localScale = new Vector3(size.x - 0.1f, 0.6f, size.y - 0.1f);
            //cellIndicatorRenderer.material.mainTextureScale = size;
        }

        previewSelector.transform.GetChild(0).GetComponent<Renderer>().material = previewMaterialInstance;
    }

    public void StopPreview()
    {
        system.cellIndicator.SetActive(false);
        if (previewObject != null)
            GameObject.Destroy(previewObject);
            
        GameObject.Destroy(previewSelector);

        materials.Clear();
        previewObject = null;
        previewSelector = null;        
    }

    private void MoveCursor(Vector3 position)
    {
        Vector3 localCursorOffsetTranslate = new();

        if (previewObject != null)
            localCursorOffsetTranslate = previewObject.transform.TransformDirection(new Vector3(cursorOffset.x, 0, cursorOffset.y));

        system.cellIndicator.transform.position = new Vector3(position.x + localCursorOffsetTranslate.x, position.y + previewYOffset, position.z + localCursorOffsetTranslate.z);
        previewSelector.transform.position = new Vector3(position.x + localCursorOffsetTranslate.x, position.y, position.z + + localCursorOffsetTranslate.z);
    }

    private void MovePreview(Vector3 position)
    {
        previewObject.transform.position = new Vector3(
            position.x,
            position.y,
            position.z
        );
    }

    private void RotateCursor(float rotation)
    {
        system.cellIndicator.transform.rotation = Quaternion.Euler(0, rotation, 0);
        previewSelector.transform.rotation = Quaternion.Euler(0, rotation, 0);
    }

    private void RotatePreview(float rotation)
    {
        previewObject.transform.rotation = Quaternion.Euler(0, rotation, 0);
    }

    public void UpdatePreview(Vector3 pos, Vector2Int size, int cost, float rotation)
    {
        system.cellIndicator.SetActive(true);
        if (previewObject != null)
        {
            MovePreview(pos);
            RotatePreview(rotation);
        }

        MoveCursor(pos);
        RotateCursor(rotation);
        system.GetBuildToolsUI().AdjustLabels(cost, size);
        previewPos = pos;
    }

    public void RefreshColors()
    {
        int index = 0;
        for (int i = 0; i < previewObject.GetComponentsInChildren<Renderer>().Length; i++)
        {
            if (previewObject.GetComponentsInChildren<Renderer>()[i] != previewSelector.GetComponentInChildren<Renderer>())
            {
                for (int j = 0; j < previewObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length; j++)
                {
                    previewObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials[j].color = materials[index].color;
                    index++;
                }
            }
        }
    }

    public Vector2Int GetPreviewSize()
    {
        return previewSize;
    }

    public bool GetExpansionState()
    {
        return false;
    }

    public void Deselect()
    {
        return;
    }
}