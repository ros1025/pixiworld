using UnityEngine;

public interface IBuildingState
{
    void EndState();
    void OnModify(Vector3 gridPosition, int rotation = 0);
    void OnAction(Vector3 gridPosition);
    void UpdateState(Vector3 gridPosition, int rotation = 0);
}