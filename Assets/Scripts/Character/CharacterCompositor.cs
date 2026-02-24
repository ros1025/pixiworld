using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem.XR;

public class CharacterCompositor : MonoBehaviour
{
    public SkinnedMeshRenderer body;

    private void GetRecursiveBones(List<Transform> bones, Transform currentBone)
    {
        bones.Add(currentBone);

        for (int i = 0; i < currentBone.childCount; i++)
        {
            GetRecursiveBones(bones, currentBone.GetChild(i));
        }
    }
}
