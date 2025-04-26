using UnityEngine;
using UnityEditor;

public class CombineAndSaveMeshes : MonoBehaviour
{
    [MenuItem("Tools/Combine Meshes and Save")]
    static void CombineAndSaveMeshese()
    {
        GameObject selected = Selection.activeGameObject;
        var selectedFile = Selection.activeObject;
        if (selected == null)
        {
            Debug.LogError("No game object selected.");
            return;
        }

        // 자식들의 MeshFilter를 가져옴
        MeshFilter[] meshFilters = selected.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            Mesh meshCopy = Instantiate(meshFilters[i].sharedMesh);
            /*for (int j = 0; j < meshCopy.subMeshCount; j++)
            {
                meshCopy.
            }*/
            combine[i].mesh = meshCopy;
        }

        // 새로운 병합된 Mesh 생성
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);

        // 병합된 Mesh를 에셋으로 저장
        string path = AssetDatabase.GetAssetPath(selectedFile);
        AssetDatabase.CreateAsset(combinedMesh, path);
        AssetDatabase.SaveAssets();

        Debug.Log("Mesh has been combined and saved to " + path);
    }
}



