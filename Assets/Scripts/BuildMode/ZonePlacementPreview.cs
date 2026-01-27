using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZonePlacementPreview : IStaticPreviewSystem
{
    private float previewYOffset = 0.05f;

    private Vector2 cursorOffset;

    public GameObject previewObject;

    public GameObject previewSelector;

    private InputManager input;
    private PlacementSystem system;

    private Material previewMaterialInstance;
    private Color selectedColor;

    private Vector3 previewPos;
    private bool expand;
    private Vector2Int previewSize;

    public List<GameObject> expanders = new();
    public GameObject selectedCursor;
    public Vector2Int minSize;

    public void StartPreview(Vector3 originPos, Vector2Int size, Vector2Int minSize, PlacementSystem system, InputManager input)
    {
        this.system = system;
        this.input = input;
        previewMaterialInstance = new Material(system.previewMaterialPrefab);
        selectedColor = Color.darkGreen;

        previewSelector = GameObject.Instantiate(system.previewSelectorObject);
        previewSelector.transform.name = "Selector";
        previewPos = originPos;
        previewSize = size;
        this.minSize = minSize;
        PrepareExpandingCursor(originPos, size);
        MoveCursor(originPos);
        system.cellIndicator.SetActive(true);
    }

    private void PrepareExpandingCursor(Vector3 originPos, Vector2Int size)
    {
        system.cellIndicator.transform.localScale = new Vector3(size.x, size.y, size.y);
        previewSelector.transform.localPosition = new Vector3(0.05f, 0f, 0.05f);
        previewSelector.transform.localScale = new Vector3(size.x - 0.1f, 0.6f, size.y - 0.1f);
        previewSelector.transform.GetChild(0).GetComponent<Renderer>().material = previewMaterialInstance;

        system.expanderParent.transform.position = new Vector3(originPos.x, 0.01f, originPos.z);
        system.expanderParent.transform.rotation = Quaternion.Euler(0, 0, 0);

        GameObject horizontalCursor = GameObject.Instantiate(system.expandingCursor, new Vector3(originPos.x + size.x, 0.01f, originPos.z + (size.y/2)), Quaternion.Euler(0, 0, 90));
        horizontalCursor.transform.name = "horizontal"; expanders.Add(horizontalCursor);

        GameObject verticalCursor = GameObject.Instantiate(system.expandingCursor, new Vector3(originPos.x + (size.x/2), 0.01f, originPos.z + size.y), Quaternion.Euler(0, 90, 90));
        verticalCursor.transform.name = "vertical"; expanders.Add(verticalCursor);

        foreach (GameObject item in expanders) 
            item.transform.SetParent(system.expanderParent.transform);
    }

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
        bool ans = false;
        foreach (GameObject expander in expanders)
        {
            if (input.RayHitObject(expander))
            {
                ans = true;
                selectedCursor = expander;
                selectedCursor.GetComponent<Renderer>().material.color = selectedColor;
            }
        }
        return ans;
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

    public Vector2Int GetPreviewSize()
    {
        return previewSize;
    }

    public void SetExpansionState(bool state)
    {
        this.expand = state;
    }

    public void StopPreview()
    {
        system.cellIndicator.SetActive(false);
        foreach (GameObject expander in expanders)
        {
            GameObject.Destroy(expander);
        }

        if (previewObject != null)
            GameObject.Destroy(previewObject);
            
        GameObject.Destroy(previewSelector);

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
        system.expanderParent.transform.position = new Vector3(position.x + localCursorOffsetTranslate.x, position.y + previewYOffset * 2, position.z + localCursorOffsetTranslate.z);
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
        system.expanderParent.transform.rotation = Quaternion.Euler(0, rotation, 0);
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

    public void UpdateSize(Vector3 position)
    {
        Vector3 newSize = system.expanderParent.transform.InverseTransformPoint(position);
        Vector3 horizontalPos = system.expanderParent.transform.Find("horizontal").transform.localPosition;
        Vector3 verticalPos = system.expanderParent.transform.Find("vertical").transform.localPosition;

        if (selectedCursor.name == "horizontal")
        {
            previewSize = new Vector2Int(Mathf.RoundToInt(newSize.x), previewSize.y);
            system.expanderParent.transform.Find("horizontal").transform.SetLocalPositionAndRotation(new Vector3(Mathf.RoundToInt(newSize.x), horizontalPos.y, horizontalPos.z), Quaternion.Euler(0, 0, 90));
            system.expanderParent.transform.Find("vertical").transform.SetLocalPositionAndRotation(new Vector3(Mathf.RoundToInt(newSize.x)/ 2, verticalPos.y, verticalPos.z), Quaternion.Euler(0, 90, 90));
        }
        else if (selectedCursor.name == "vertical")
        {
            previewSize = new Vector2Int(previewSize.x, Mathf.RoundToInt(newSize.z));
            system.expanderParent.transform.Find("horizontal").transform.SetLocalPositionAndRotation(new Vector3(horizontalPos.x, horizontalPos.y, Mathf.RoundToInt(newSize.z) / 2), Quaternion.Euler(0, 0, 90));
            system.expanderParent.transform.Find("vertical").transform.SetLocalPositionAndRotation(new Vector3(verticalPos.x, verticalPos.y, Mathf.RoundToInt(newSize.z)), Quaternion.Euler(0, 90, 90));
        }

        system.cellIndicator.transform.localScale = new Vector3(previewSize.x, previewSize.y, previewSize.y);
        previewSelector.transform.localScale = new Vector3(previewSize.x - 0.1f, 0.6f, previewSize.y - 0.1f);
    }

    public bool GetExpansionState()
    {
        return expand;
    }

    public void Deselect()
    {
        foreach (GameObject expander in expanders)
        {
            expander.GetComponent<Renderer>().material.color = system.expandingCursor.GetComponent<Renderer>().sharedMaterial.color;
        }
    }
}
