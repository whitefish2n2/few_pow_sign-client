#if UNITY_EDITOR
using UnityEngine;

using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Codes.Util
{
    public class ExtractVertices : MonoBehaviour
    {
        [MenuItem("Tools/ExtractVertices")]
        public static void Extract()
        {
            if (File.Exists("Assets/MapPoly/newMap.txt"))
            {
                Debug.Log("기본 경로에 이미 같은 이름의 파일이 존재합니다. Assets/MapPoly/newMap.txt 파일의 이름을 변경한 후 사용하세요");
                return;
            }
            GameObject[] objs = Selection.gameObjects;
            if (objs.Length == 0)
            {
                Debug.LogError("No game object selected.");
                return;
            }
            var sb = new StringBuilder();
            foreach (var o in objs)
            {
                sb.AppendLine("v");
                var meshFilter = o.GetComponent<MeshFilter>();
                if (meshFilter == null || meshFilter.sharedMesh == null) continue;
                var mesh = meshFilter.sharedMesh;
                foreach (var m in mesh.vertices)
                {
                    o.transform.TransformPoint(m);
                    sb.AppendLine($"{m.x},{m.y},{m.z}");
                }

                sb.AppendLine("t");

                for (int i = 0; i < mesh.triangles.Length; i += 3)
                {
                    sb.AppendLine(mesh.triangles[i + 0] + "," + mesh.triangles[i + 1] + "," + mesh.triangles[i + 2]);
                }
                sb.AppendLine("-");
            }
            File.WriteAllText("Assets/MapPoly/newMap.mapverticesinfo", sb.ToString());

            Debug.Log("맵 데이터가 성공적으로 생성되었습니다.");
        }
    }
}
#endif