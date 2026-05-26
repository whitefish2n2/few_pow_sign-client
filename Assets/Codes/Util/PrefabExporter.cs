using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Codes.Util
{
    public class PrefabExporter : EditorWindow
    {
        // --- [설정 변수] ---
        private GameObject targetObject;
        private bool includeChildren = true;

        // ID 발급용 키
        private const string ID_COUNTER_KEY = "DynamicPrefab_LastID";

        // --- [메뉴 항목] ---
        [MenuItem("Tools/Open Prefab Exporter")]
        public static void ShowWindow()
        {
            GetWindow<PrefabExporter>("Prefab Exporter");
        }

        // 우클릭 시 UI 창을 띄우고 타겟을 자동 할당
        [MenuItem("GameObject/Export Object Data", false, 0)]
        [MenuItem("Assets/Export Object Data", false, 0)]
        public static void ExportFromContextMenu()
        {
            if (Selection.activeGameObject != null)
            {
                PrefabExporter window = GetWindow<PrefabExporter>("Prefab Exporter");
                window.targetObject = Selection.activeGameObject;
                window.Focus();
            }
            else
            {
                Debug.LogWarning("선택된 GameObject가 없습니다.");
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("Dynamic Prefab Exporter", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            targetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true);
            EditorGUILayout.Space();
            
            includeChildren = EditorGUILayout.Toggle("Include Children", includeChildren);
            EditorGUILayout.HelpBox("체크 해제 시 자식 오브젝트들을 프리팹과 서버 데이터에서 완전히 제외합니다.", MessageType.None);
            
            int nextId = EditorPrefs.GetInt(ID_COUNTER_KEY, 1);
            EditorGUILayout.LabelField($"Next Prefab ID: {nextId}");

            EditorGUILayout.Space(20);

            GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);
            if (GUILayout.Button("Export & Create Prefab", GUILayout.Height(40)))
            {
                if (targetObject == null)
                {
                    EditorUtility.DisplayDialog("Error", "Export할 Target Object를 할당해주세요.", "OK");
                    return;
                }
                ExportProcess(targetObject, includeChildren);
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Reset ID Counter (위험)", GUILayout.Height(20)))
            {
                if (EditorUtility.DisplayDialog("경고", "ID 카운터를 1로 초기화하시겠습니까?", "초기화", "취소"))
                {
                    EditorPrefs.SetInt(ID_COUNTER_KEY, 1);
                }
            }
        }

        // --- [Export Logic] ---
        private static void ExportProcess(GameObject rootObj, bool exportChildren)
        {
            // 1. 고유 ID 발급
            int currentId = EditorPrefs.GetInt(ID_COUNTER_KEY, 1);
            EditorPrefs.SetInt(ID_COUNTER_KEY, currentId + 1);

            string fileName = $"{rootObj.name}-{currentId}";

            // 2. 폴더 구조 생성 (Assets/DynamicPrefab/ServerPrefab)
            EnsureFolderExists("Assets", "DynamicPrefab");
            EnsureFolderExists("Assets/DynamicPrefab", "ServerPrefab");

            string clientPrefabPath = $"Assets/DynamicPrefab/{fileName}.prefab";
            string serverTxtDirPath = Path.Combine(Application.dataPath, "DynamicPrefab/ServerPrefab");
            string serverTxtFilePath = Path.Combine(serverTxtDirPath, $"{fileName}.objectPrefab");

            // 3. 자식 포함 여부에 따른 오브젝트 전처리 (includeChildren 대응 복사본 생성)
            GameObject tempObj = Instantiate(rootObj);
            tempObj.name = rootObj.name;

            if (!exportChildren)
            {
                // 자식 오브젝트들을 완전히 제거하여 껍데기 분리
                for (int i = tempObj.transform.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(tempObj.transform.GetChild(i).gameObject);
                }
            }

            // 4. 클라이언트용 프리팹 에셋 생성 (.prefab)
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(tempObj, clientPrefabPath);
            
            // 프리팹 에셋 저장이 끝났으므로 씬의 임시 오브젝트는 즉시 해제
            DestroyImmediate(tempObj);

            if (savedPrefab == null)
            {
                Debug.LogError("클라이언트 프리팹 저장에 실패했습니다: " + clientPrefabPath);
                return;
            }

            // 5. 서버용 텍스트 파일 (.objectPrefab) 추출 작성 -> GameObjectArgument 구조에 대응
            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine("[SECTION: HEADER]");
            sb.AppendLine($"PrefabID: {currentId}");
            sb.AppendLine($"PrefabName: {fileName}");
            sb.AppendLine();
            
            sb.AppendLine("[SECTION: OBJECTS]");

            // 생성 완료된 프리팹 에셋 기준으로만 트랜스폼 순회 (자식 제외 시 루트 1개만 수집됨)
            List<GameObject> exportList = new List<GameObject>();
            Transform[] transforms = savedPrefab.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in transforms)
            {
                exportList.Add(t.gameObject);
            }

            foreach (GameObject go in exportList)
            {
                sb.AppendLine("-");
                sb.AppendLine($"Name: {go.name}");
                sb.AppendLine($"Tag: {go.tag}");
                sb.AppendLine($"LayerIndex: {go.layer}");
                sb.AppendLine($"LayerName: {LayerMask.LayerToName(go.layer)}");

                Transform t = go.transform;
                // 프리팹 에셋 기준이므로 루트는 (0,0,0), 자식들은 루트 기준 상대적 로컬 좌표로 자연스럽게 정렬됨
                sb.AppendLine($"Position: {FormatVec3(t.position)}");
                sb.AppendLine($"Rotation: {FormatQuat(t.rotation)}");
                sb.AppendLine($"Scale: {FormatVec3(t.lossyScale)}");

                ExportCollider(go, sb);
                ExportRigidbody(go, sb);
                ExportServerComponents(go, sb);
            }

            // 서버용 텍스트 매니페스트 파일만 작성 (.prefab 바이너리 복사는 제외)
            File.WriteAllText(serverTxtFilePath, sb.ToString());
            
            // 프로젝트 뷰 동기화
            AssetDatabase.Refresh();
            
            Debug.Log($"<color=green><b>[Export 완료]</b></color> ID: {currentId} / Name: {fileName}\n" +
                      $"▶ 클라이언트용 유니티 프리팹: {clientPrefabPath}\n" +
                      $"▶ 서버용 텍스트 프리팹 데이터: {serverTxtFilePath}");
        }

        private static void EnsureFolderExists(string parentFolder, string newFolderName)
        {
            string fullPath = $"{parentFolder}/{newFolderName}";
            if (!AssetDatabase.IsValidFolder(fullPath))
            {
                AssetDatabase.CreateFolder(parentFolder, newFolderName);
            }
        }

        // --- 파싱 헬퍼 함수들 ---
        private static void ExportCollider(GameObject go, StringBuilder sb)
        {
            Collider col = go.GetComponent<Collider>();
            if (col == null || !col.enabled) return;

            if (col is BoxCollider box)
            {
                sb.AppendLine("COMPONENT: BoxCollider");
                sb.AppendLine($"IsTrigger: {(box.isTrigger ? "1" : "0")}");
                sb.AppendLine($"Center: {FormatVec3(box.center)}");
                sb.AppendLine($"Size: {FormatVec3(box.size)}");
            }
            else if (col is SphereCollider sphere)
            {
                sb.AppendLine("COMPONENT: SphereCollider");
                sb.AppendLine($"IsTrigger: {(sphere.isTrigger ? "1" : "0")}");
                sb.AppendLine($"Center: {FormatVec3(sphere.center)}");
                sb.AppendLine($"Radius: {sphere.radius:F4}");
            }
            else if (col is CapsuleCollider capsule)
            {
                sb.AppendLine("COMPONENT: CapsuleCollider");
                sb.AppendLine($"IsTrigger: {(capsule.isTrigger ? "1" : "0")}");
                sb.AppendLine($"Center: {FormatVec3(capsule.center)}");
                sb.AppendLine($"Radius: {capsule.radius:F4}");
                sb.AppendLine($"Height: {capsule.height:F4}");
                sb.AppendLine($"Direction: {capsule.direction}");
            }
            else if (col is MeshCollider meshCol)
            {
                Mesh mesh = meshCol.sharedMesh;
                if (mesh == null) return;
                sb.AppendLine("COMPONENT: MeshCollider");
                sb.AppendLine($"IsTrigger: {(meshCol.isTrigger ? "1" : "0")}");
                sb.AppendLine($"VertexCount: {mesh.vertexCount}");
                foreach (Vector3 v in mesh.vertices) sb.AppendLine($"{v.x:F4},{v.y:F4},{v.z:F4}");
                int[] triangles = mesh.triangles;
                sb.AppendLine($"TriangleCount: {triangles.Length / 3}");
                for (int i = 0; i < triangles.Length; i += 3)
                    sb.AppendLine($"{triangles[i]},{triangles[i+1]},{triangles[i+2]}");
            }
        }

        private static void ExportRigidbody(GameObject go, StringBuilder sb)
        {
            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb == null) return;

            sb.AppendLine("COMPONENT: Rigidbody");
            sb.AppendLine($"Mass: {rb.mass:F4}");
            sb.AppendLine($"Drag: {rb.linearDamping:F4}");
            sb.AppendLine($"AngularDrag: {rb.angularDamping:F4}");
            sb.AppendLine($"UseGravity: {(rb.useGravity ? "1" : "0")}");
            sb.AppendLine($"IsKinematic: {(rb.isKinematic ? "1" : "0")}");
            sb.AppendLine($"Constraints: {(int)rb.constraints}");
            sb.AppendLine($"CollisionDetection: {(int)rb.collisionDetectionMode}");
            sb.AppendLine($"CenterOfMass: {FormatVec3(rb.centerOfMass)}");
        }

        private static void ExportServerComponents(GameObject go, StringBuilder sb)
        {
            ServerComponent[] serverComps = go.GetComponents<ServerComponent>();
            foreach (var comp in serverComps)
            {
                if (!comp.enabled) continue;
                sb.AppendLine($"COMPONENT: {comp.GetType().Name}");
                string data = comp.Serialize();
                if (!string.IsNullOrWhiteSpace(data)) sb.AppendLine(data);
                if (string.IsNullOrWhiteSpace(data)) Debug.LogError($"Parse Failed COMPONENT: {comp.GetType().Name} on {go.name}");
            }
        }

        private static string FormatVec3(Vector3 v) => $"{v.x:F4},{v.y:F4},{v.z:F4}";
        private static string FormatQuat(Quaternion q) => $"{q.x:F4},{q.y:F4},{q.z:F4},{q.w:F4}";
    }
}
#endif