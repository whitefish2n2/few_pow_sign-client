using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if(UNITY_EDITOR)
namespace Codes.Util
{
    public class ExtractGroundVertices : MonoBehaviour
    {
        [MenuItem("Tools/Export Ground Collider GameObject")]
        static void ExportColliders()
        {
            string exportDir = Path.Combine(Application.dataPath, "../ExportedColliderGameobjects");
            if (!Directory.Exists(exportDir))
                Directory.CreateDirectory(exportDir);
            
            int index = 1;
            string path;
            do
            {
                path = Path.Combine(exportDir, $"colliders_{index}.txt");
                index++;
            } while (File.Exists(path));
            
            var colliders = GameObject.FindObjectsByType<Collider>(FindObjectsSortMode.InstanceID)
                .Where((a)=>a.CompareTag("Ground") &&
                            a.gameObject.isStatic &&
                            !a.isTrigger);
            var exportData = new List<string>();

            foreach (var col in colliders)
            {
                if (col is BoxCollider box)
                {
                    Vector3 pos = box.transform.position;
                    Vector3 size = Vector3.Scale(box.size, box.transform.lossyScale);
                    Vector3 rot = box.transform.rotation.eulerAngles;
                    exportData.Add($"-BOX {col.gameObject.name} {col.gameObject.tag} {pos.x:F3} {pos.y:F3} {pos.z:F3} {size.x:F3} {size.y:F3} {size.z:F3} {rot.x:F3} {rot.y:F3} {rot.z:F3}");
                }
                else if (col is MeshCollider meshCol)
                {
                    Mesh mesh = meshCol.sharedMesh;
                    var verts = mesh.vertices;
                    var tris = mesh.triangles;

                    exportData.Add($"-MESH {col.gameObject.name} {col.gameObject.tag} {mesh.name} {verts.Length} {tris.Length}");
                    string vertsString = " ";
                    foreach (var v in verts)
                    {
                        vertsString += $"{v.x} {v.y} {v.z} {v} ";
                    }
                    string trisString = "";
                    foreach (var t in tris)
                    {
                        trisString += $"{t} ";
                    }
                    exportData.Add(vertsString);
                    exportData.Add(trisString);
                }
                else if (col is CapsuleCollider cap)
                {
                    Vector3 pos = cap.transform.position;
                    exportData.Add($"-CAPSULE {col.gameObject.name} {col.gameObject.tag} {pos.x:F3} {pos.y:F3} {pos.z:F3} {cap.radius:F3} {cap.height:F3}");
                }
            }

            File.WriteAllLines("Assets/Export/collider_data.txt", exportData);
            Debug.Log($"Exported {exportData.Count} colliders.");
        }
    }
}
#endif