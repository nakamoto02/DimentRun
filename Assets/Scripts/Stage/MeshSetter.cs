using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class MeshSetter : MonoBehaviour
{
    MeshFilter   meshFilter;
    MeshCollider meshCollider;

    public void SetMesh(Mesh rMesh, Mesh cMesh)
    {
        if (meshFilter   == null) meshFilter   = GetComponent<MeshFilter>();
        if (meshCollider == null) meshCollider = GetComponent<MeshCollider>();

        meshFilter.sharedMesh   = rMesh;
        meshCollider.sharedMesh = cMesh;
    }
}
