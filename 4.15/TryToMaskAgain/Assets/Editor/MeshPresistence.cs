using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NewBehaviourScript
{
    [MenuItem("Tools/Mesh/Presistence")]
	public static void Presistence()
    {
        GameObject selectObj = Selection.activeGameObject;
        MeshFilter meshFilter = selectObj.GetComponent<MeshFilter>();
        meshFilter.mesh = selectObj.GetComponent<GenerateLowPoly>().Generate();
    }
}
