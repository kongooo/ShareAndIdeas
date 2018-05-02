using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lighting : MonoBehaviour {

    public LayerMask wall;
    public float radius = 10;
    public int interval = 5;
    public GameObject mask;

    private Mesh newmesh;
    private Vector3 direction;
    private Vector3[] verts;
    private int[] triangles;
	
	void Start () {
        newmesh = new Mesh();
        mask.GetComponent<MeshFilter>().mesh = newmesh;
        verts = new Vector3[360 / interval + 1];
        triangles = new int[3 * 360 / interval];
	}

	void Update () {
        Drawline();
	}

    void Drawline()
    {
        int index = 0;
        int triIndex = 0;
        verts[index] = transform.position;
        for (var a = 0; a < 360; a += interval)
        {
            index++;
            //角度转弧度
            direction = new Vector3(Mathf.Sin(Mathf.Deg2Rad * a), 0, Mathf.Cos(Mathf.Deg2Rad * a));
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, radius, wall))
            {
                Debug.DrawLine(transform.position, hit.point, Color.red);
                verts[index] = hit.point;
            }
            else
                verts[index] = transform.position + direction * radius;
        }          
        for(int i=1;i<triangles.Length/3;i++)
        {
            triangles[triIndex] = 0;
            triangles[triIndex + 1] = i;
            triangles[triIndex + 2] = i + 1;
            triIndex += 3;
        }
        //画出最后一个三角形
        triangles[triIndex] = 0;
        triangles[triIndex + 1] = triangles.Length / 3;
        triangles[triIndex + 2] = 1;
        
        newmesh.Clear();
        newmesh.vertices = verts;
        newmesh.triangles = triangles;
        newmesh.RecalculateNormals();
    }
}
