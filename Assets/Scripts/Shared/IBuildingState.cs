using UnityEngine;

public interface IBuildingState
{
    void EndState();
    void OnModify(Vector3Int gridPosition, int rotation = 0);
    void OnAction(Vector3Int gridPosition);
    void UpdateState(Vector3Int gridPosition, int rotation = 0);
}