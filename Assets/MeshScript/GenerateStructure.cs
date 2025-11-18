using UnityEngine;


public class GenerateStructure : MonoBehaviour
{
    public Mesh mesh;
    public float width, height, depth;
    public Vector3[] vertexs ;
    public int vertexCount;
    public int triangles;

    float
        minX = Mathf.Infinity, maxX = -Mathf.Infinity,
        minY = Mathf.Infinity, maxY = -Mathf.Infinity,
        minZ = Mathf.Infinity, maxZ = -Mathf.Infinity;

    private void Start()
    {
        GetComponent<MeshFilter>().mesh = mesh;
        vertexCount = mesh.vertexCount;
    }
    private void Update()
    {
        for (int i = 0; i < vertexCount; i++)
        {
            vertexs[i] = mesh.vertices[i];
            Debug.Log(vertexs[i]);
        }

        foreach (Vector3 ver in vertexs) {
            if (ver.x > maxX)       maxX = ver.x;
            else if (ver.x < minX)  minX = ver.x;

            if (ver.y > maxY)       maxY = ver.y;
            else if (ver.y < minY)  minY = ver.y;

            if (ver.z > maxZ)       maxZ = ver.z;
            else if (ver.z < minZ)  minZ = ver.z;
        }
        width = maxX - minX;
        height = maxY - minY;
        depth = maxZ - minZ;

        Debug.Log(width);
        Debug.Log(height);
        Debug.Log(depth);

    }
}
