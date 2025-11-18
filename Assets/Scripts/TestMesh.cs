using System.Linq;
using UnityEditor;
using UnityEngine;

public class TestMesh : MonoBehaviour
{
    [SerializeField]
    private GameObject obj;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MeshFilter[] meshes = obj.GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter mesh in meshes)
        {
            for (int i = 0; i < mesh.mesh.vertices.Count(); i++)
            {
                Debug.Log($"{i} : {mesh.mesh.vertices[i]}");
            }

            for (int i = 0; i < mesh.mesh.triangles.Count(); i++)
            {
                Debug.Log($"{i} : {mesh.mesh.triangles[i]}");
            }
        }
    }
}
