using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
public class StoneGenerator : EditorWindow
{

    bool applyScale = true;

    [MenuItem("File/Export/Stone")]
    public static void ShowWindow()
    {
        var win = EditorWindow.GetWindow(typeof(StoneGenerator));
        win.titleContent.text = "Stone Generator";
    }


    private string Path= "";
    private int Num = 0;
    void OnGUI()
    {
        GUILayout.Label("Generate Stone", EditorStyles.boldLabel);
        Path = EditorGUILayout.TextField("Save to", Path);
        if (GUILayout.Button("Select Folder"))
        {
            Path = EditorUtility.SaveFolderPanel("Save to Folder", Path, "");
        }
        GUILayout.Space(10);
        Num = EditorGUILayout.IntField("Num of Stone", Num);
        if (GUILayout.Button("Generate"))
        {
            if (string.IsNullOrEmpty(Path))
            {
                EditorUtility.DisplayDialog("Path is Empty", "Please Select a Path", "OK");
            }
            else
            {
                StartExport(Num,Path);
            }
        }
    }


    void StartExport(int count, string Path)
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Start Unity", "Start Play Mode to and click again", "OK");
            return;
        }
        EditorUtility.DisplayCancelableProgressBar("Exporting OBJ", "Please wait.. Starting export.", 0);
        var generator = FindObjectOfType<Generator>();
        for (int i = 0; i < count; i++)
        {
            //randomize
            generator.RandomSeed();
            generator.Regenerate(generator.seed);
            //Export
            Export(System.IO.Path.Combine(Path, (i+1).ToString("D6") + ".obj"));
            if (EditorUtility.DisplayCancelableProgressBar("Exporting OBJ", "Please wait.. Starting export.", ((float)(i) / count)))
            {
                Debug.Log("cancel");
                break;
            }
        }
        EditorUtility.ClearProgressBar();
    }

    void Export(string exportPath)
    {
        //init stuff
        Dictionary<string, bool> materialCache = new Dictionary<string, bool>();
        var exportFileInfo = new System.IO.FileInfo(exportPath);
        string baseFileName = System.IO.Path.GetFileNameWithoutExtension(exportPath);

        //get list of required export things
        MeshFilter[] sceneMeshes;
        sceneMeshes = FindObjectsOfType(typeof(MeshFilter)) as MeshFilter[];

        if (Application.isPlaying)
        {
            foreach (MeshFilter mf in sceneMeshes)
            {
                MeshRenderer mr = mf.gameObject.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    if (mr.isPartOfStaticBatch)
                    {
                        return;
                    }
                }
            }
        }

        //work on export
        StringBuilder sb = new StringBuilder();
        StringBuilder sbMaterials = new StringBuilder();
        float maxExportProgress = (float)(sceneMeshes.Length + 1);
        int lastIndex = 0;
        for (int i = 0; i < sceneMeshes.Length; i++)
        {
            string meshName = sceneMeshes[i].gameObject.name;
            float progress = (float)(i + 1) / maxExportProgress;
            MeshFilter mf = sceneMeshes[i];
            MeshRenderer mr = sceneMeshes[i].gameObject.GetComponent<MeshRenderer>();

            //export the meshhh :3
            Mesh msh = mf.sharedMesh;
            int faceOrder = (int)Mathf.Clamp((mf.gameObject.transform.lossyScale.x * mf.gameObject.transform.lossyScale.z), -1, 1);

            //export vector data (FUN :D)!
            foreach (Vector3 vx in msh.vertices)
            {
                Vector3 v = vx;
                if (applyScale)
                {
                    v = MultiplyVec3s(v, mf.gameObject.transform.lossyScale);
                }
                v.x *= -1;
                sb.AppendLine("v " + v.x + " " + v.y + " " + v.z);
            }
            foreach (Vector3 vx in msh.normals)
            {
                Vector3 v = vx;

                if (applyScale)
                {
                    v = MultiplyVec3s(v, mf.gameObject.transform.lossyScale.normalized);
                }
                v.x *= -1;
                sb.AppendLine("vn " + v.x + " " + v.y + " " + v.z);

            }
            foreach (Vector2 v in msh.uv)
            {
                sb.AppendLine("vt " + v.x + " " + v.y);
            }

            for (int j = 0; j < msh.subMeshCount; j++)
            {
                if (mr != null && j < mr.sharedMaterials.Length)
                {
                    string matName = mr.sharedMaterials[j].name;
                    sb.AppendLine("usemtl " + matName);
                }
                else
                {
                    sb.AppendLine("usemtl " + meshName + "_sm" + j);
                }

                int[] tris = msh.GetTriangles(j);
                for (int t = 0; t < tris.Length; t += 3)
                {
                    int idx2 = tris[t] + 1 + lastIndex;
                    int idx1 = tris[t + 1] + 1 + lastIndex;
                    int idx0 = tris[t + 2] + 1 + lastIndex;
                    if (faceOrder < 0)
                    {
                        sb.AppendLine("f " + ConstructOBJString(idx2) + " " + ConstructOBJString(idx1) + " " + ConstructOBJString(idx0));
                    }
                    else
                    {
                        sb.AppendLine("f " + ConstructOBJString(idx0) + " " + ConstructOBJString(idx1) + " " + ConstructOBJString(idx2));
                    }

                }
            }

            lastIndex += msh.vertices.Length;
        }

        //write to disk
        System.IO.File.WriteAllText(exportPath, sb.ToString());
        //export complete, close progress dialog
    }
    Vector3 RotateAroundPoint(Vector3 point, Vector3 pivot, Quaternion angle)
    {
        return angle * (point - pivot) + pivot;
    }
    Vector3 MultiplyVec3s(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
    }
    private string ConstructOBJString(int index)
    {
        string idxString = index.ToString();
        return idxString + "/" + idxString + "/" + idxString;
    }

}
