using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SubmeshBlender : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GetComponent<MeshFilter>().mesh.subMeshCount = 2;
        int[] set1 = new int[GetComponent<MeshFilter>().mesh.triangles.Length / 2 + 1];
        int[] set2 = new int[GetComponent<MeshFilter>().mesh.triangles.Length / 2 + 1];

        int otherCount = 0;
        int set1Count = 0;
        int set2Count = 0;
        for (int i = 0; i < GetComponent<MeshFilter>().mesh.triangles.Length; i++)
        {
            if(otherCount == 6)
            {
                otherCount = 0;
            }

            if(otherCount < 3)
            {
                set1[set1Count] = GetComponent<MeshFilter>().mesh.triangles[i];
                set1Count++;
            }
            else
            {
                set2[set2Count] = GetComponent<MeshFilter>().mesh.triangles[i];
                set2Count++;
            }

            otherCount++;
        }
        int size = set1.Length % 3;
        Array.Resize(ref set1, set1.Length - size);
        size = set2.Length % 3;
        Array.Resize(ref set2, set2.Length - size);
        Debug.Log(set1.Length);
        Debug.Log(set2.Length);

        GetComponent<MeshFilter>().mesh.SetTriangles(set2, 0);
        GetComponent<MeshFilter>().mesh.SetTriangles(set1, 1);
        //GetComponent<MeshFilter>().mesh.SetTriangles(GetComponent<MeshFilter>().mesh.triangles.ToArray(), 2);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
