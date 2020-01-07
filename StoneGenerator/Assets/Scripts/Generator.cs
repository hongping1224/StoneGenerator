using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DateTime = System.DateTime;
public class Generator : MonoBehaviour
{
    public int seed = 37823787;
    private Material mat;
    private Vector3 center;
    private Mesh ori;
    private float offsetmax = 20;
    public float offsetPercent = 0.05f;


    private void OnGUI()
    {
        if (GUILayout.Button("GenerateStone"))
        {

        }
    }

    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        ori = new Mesh();
        ori.vertices = mesh.vertices;
        ori.triangles = mesh.triangles;
        ori.uv = mesh.uv;
        ori.normals = mesh.normals;
        ori.colors = mesh.colors;
        ori.tangents = mesh.tangents;
        Regenerate(seed);
    }

    private int oldseed = 0;
    private void Update()
    {
        if(oldseed != seed)
        {
            oldseed = seed;
            GetComponent<MeshFilter>().mesh = ori;
            Regenerate(oldseed);
        }
    }

    public void RandomSeed()
    {
        int newSeed = (int)(DateTime.Now.Ticks % (long)int.MaxValue);
        seed = newSeed;
    }

    public void Regenerate(int seed)
    {
        Random.InitState(seed);
        float xoffset = Random.Range(0, 10000);
        float yoffset = Random.Range(0, 10000);
        float maxaxis = float.MinValue;
        float xscale = Random.Range(0.1f, 0.4f);
        maxaxis = Mathf.Max(maxaxis, xscale);
        float yscale = Random.Range(0.1f, 0.4f);
        maxaxis = Mathf.Max(maxaxis, yscale);
        float zscale = Random.Range(0.1f, 0.4f);
        maxaxis = Mathf.Max(maxaxis, zscale);
        offsetmax = maxaxis * offsetPercent;
        //offset = scale *0.05
        transform.localScale = new Vector3(xscale, yscale, zscale);
        
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> doneVerts = new List<Vector3>();
        for (int s = 0; s < ori.vertices.Length; s++)
        {
            vertices.Add(ori.vertices[s]);
        }

        center = GetComponent<Renderer>().bounds.center;

        for (int v = 0; v < vertices.Count; v++)
        {
            bool used = false;
            for (int k = 0; k < doneVerts.Count; k++)
            {
                if (doneVerts[k] == vertices[v])
                {
                    used = true;
                }
            }
            if (!used)
            {
                Vector3 curVector = vertices[v];
                doneVerts.Add(curVector);
                int smoothing =100;
                float mag = curVector.magnitude;
                float newmag = mag + Random.Range(-offsetmax, offsetmax);
                // Vector3 changedVector = (curVector + ((curVector - center) / smoothing * (Mathf.PerlinNoise(((float)v / offset)+xoffset, ((float)v / offset)+yoffset))));
                Vector3 changedVector = Vector3.Scale(curVector.normalized,new Vector3(newmag,newmag,newmag));
                for (int s = 0; s < vertices.Count; s++)
                {
                    if (vertices[s] == curVector)
                    {
                        vertices[s] = changedVector;
                    }
                }
            }
        }
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.SetVertices(vertices);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}

