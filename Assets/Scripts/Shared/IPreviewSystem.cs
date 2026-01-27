using UnityEngine;

public interface IPreviewSystem
{
    //void StartPreview();
    void StopPreview();
    void ApplyFeedback(bool validity);
    Vector3 GetPreviewPosition();
    Vector2Int GetPreviewSize();
    GameObject GetPreviewObject();
    void Deselect();
}

public interface IStaticPreviewSystem : IPreviewSystem
{
    void UpdatePreview(Vector3 pos, Vector2Int size, int cost, float rotation);
    bool CheckPreviewObject();
    bool CheckExpansionHandles();
    bool GetExpansionState();
    void SetExpansionState(bool state);
}

public interface IDynamicPreviewSystem : IPreviewSystem
{
    void ModifyPointer(int index, Vector3 pos);
    void AddPoint(int index, Vector3 pos);
    void DeletePointer(int index);
    void ClearPointer();
    bool CheckPreviewSplines();
    bool CheckExpansionHandles();
    void SetModifyState(bool state);
    bool GetModifyState();
}