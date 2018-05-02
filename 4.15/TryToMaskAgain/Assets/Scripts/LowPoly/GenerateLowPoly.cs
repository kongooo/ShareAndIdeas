using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateLowPoly : MonoBehaviour
{

    public int xSize, ySize;
    public int xCount, yCount;

    private Vector3 center;
    private Mesh lowPolyMesh;
    private Vector3[] verts;
    private int[] trians;
    private Vector3[] normals;
    private float timer = 0;
    private float speed = 1;
    private float offsetX = 0, offsetY = 0, offsetZ = 0;

    void Start()
    {
        lowPolyMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = lowPolyMesh;
        center = transform.position;
        verts = new Vector3[(xCount + 1) * (yCount + 1)];
        trians = new int[xCount * yCount * 2 * 3];
        normals = new Vector3[verts.Length];
        int xWidth = xSize / xCount;
        int yWidth = ySize / yCount;
        int index = 0;
        //初始化顶点
        for (int i = (int)center.x - xSize / 2; i <= (int)center.x + xSize / 2; i += xWidth)
            for (int j = (int)center.y - ySize / 2; j <= (int)center.y + ySize / 2; j += yWidth)
            {
                verts[index] = new Vector3(i, j, 0);
                index++;
            }
        Generate();
    }


    void Update()
    {
        Generate();
    }

    public Mesh Generate()
    {
        verts = TransformVertex(verts);
        int index = 0;
        //初始化三角形网格
        for (int i = 0; i < xCount; i++)
            for (int j = 0; j < yCount; j++)
            {
                trians[index] = j + i * (yCount + 1);
                trians[index + 1] = j + 1 + i * (yCount + 1);
                trians[index + 2] = j + (i + 1) * (yCount + 1);
                trians[index + 3] = j + (i + 1) * (yCount + 1);
                trians[index + 4] = j + 1 + i * (yCount + 1);
                trians[index + 5] = j + 1 + (i + 1) * (yCount + 1);
                index += 6;
                Vector3 v1 = verts[j + (i + 1) * (yCount + 1)] - verts[j + i * (yCount + 1)];
                Vector3 v2 = verts[j + 1 + i * (yCount + 1)] - verts[j + i * (yCount + 1)];
                Vector3 normal = Vector3.Cross(v1, v2).normalized;
                normals[j + i * (yCount + 1)] = normal;
                normals[j + 1 + i * (yCount + 1)] = normal;
                normals[j + (i + 1) * (yCount + 1)] = normal;
            }
        lowPolyMesh.Clear();
        lowPolyMesh.vertices = verts;
        lowPolyMesh.triangles = trians;
        lowPolyMesh.normals = normals;
        return lowPolyMesh;
        //lowPolyMesh.RecalculateNormals();
    }

    private Vector3[] TransformVertex(Vector3[] verts)
    {
        timer += Time.deltaTime * speed;
        for (int i = 1; i < xCount - 1; i++)
            for (int j = 1; j < yCount - 1; j++)
            {
                offsetX = Mathf.Cos(timer) / 85 * Random.Range(0.1f, 2.0f);
                offsetY = Mathf.Sin(timer) / 70 * Random.Range(0.1f, 2.0f);
                offsetZ = Mathf.Sin(timer) / 100 * Random.Range(0.1f, 2.0f);
                verts[j + i * (yCount + 1)] += new Vector3(offsetX, offsetY, offsetZ) * Random.Range(0.1f,2.0f);
            }
        return verts;
    }

    //private void OnDrawGizmos()
    //{
    //    lowPolyMesh = new Mesh();
    //    GetComponent<MeshFilter>().mesh = lowPolyMesh;
    //    center = transform.position;
    //    verts = new Vector3[(xCount + 1) * (yCount + 1)];
    //    trians = new int[xCount * yCount * 2 * 3];
    //    Generate();
    //    Gizmos.color = Color.blue;
    //    for (int i = 0; i < verts.Length; i++)
    //        Gizmos.DrawSphere(verts[i], 0.1f);
    //}

}
