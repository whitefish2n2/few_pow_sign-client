using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
namespace Codes.Util
{
    public class MapExporterWindow : EditorWindow
    {
        // --- [설정 변수] ---
        public List<string> excludedTags = new List<string> { "EditorOnly", "Untagged" }; // 무조건 제외
        public List<string> includedTags = new List<string> { "SpawnPoint", "Trigger" };  // 무조건 포함
        public LayerMask targetLayers = -1; // 나머지: 레이어 기준

        // --- [GUI 변수] ---
        private SerializedObject so;
        private SerializedProperty propLayers;
        private Vector2 scrollPosEx, scrollPosIn;

        [MenuItem("Tools/Open Map Exporter")]
        public static void ShowWindow()
        {
            GetWindow<MapExporterWindow>("Map Exporter");
        }

        private void OnEnable()
        {
            so = new SerializedObject(this);
            propLayers = so.FindProperty("targetLayers");
            LoadSettings();
        }

        private void OnDisable()
        {
            SaveSettings();
        }

        private void OnGUI()
        {
            so.Update();

            GUILayout.Label("Map Export Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // --- 1. 블랙리스트 (무조건 제외) ---
            DrawTagList("1. Always EXCLUDE Tags (Blacklist)", ref excludedTags, ref scrollPosEx, Color.red);
            EditorGUILayout.HelpBox("이 태그를 가진 오브젝트는 어떤 레이어에 있든 절대 내보내지 않습니다.", MessageType.None);
        
            EditorGUILayout.Space();
        
            // --- 2. 화이트리스트 (무조건 포함) ---
            DrawTagList("2. Always INCLUDE Tags (Whitelist)", ref includedTags, ref scrollPosIn, Color.green);
            EditorGUILayout.HelpBox("이 태그를 가진 오브젝트는 레이어 설정과 상관없이 무조건 내보냅니다.", MessageType.None);

            EditorGUILayout.Space();

            // --- 3. 레이어 설정 (기본 필터) ---
            GUI.color = Color.cyan;
            GUILayout.Label("3. Target Layers (Default Filter)", EditorStyles.boldLabel);
            GUI.color = Color.white;
            EditorGUILayout.PropertyField(propLayers, new GUIContent("Layers"));
            EditorGUILayout.HelpBox("위 태그들에 해당하지 않는 오브젝트는 이 레이어 설정을 따릅니다.", MessageType.None);

            EditorGUILayout.Space(20);

            // --- Export 버튼 ---
            GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);
            if (GUILayout.Button("Export Map Data", GUILayout.Height(40)))
            {
                Export();
            }
            GUI.backgroundColor = Color.white;

            so.ApplyModifiedProperties();
        }

        // 태그 리스트 그리는 헬퍼 함수
        private void DrawTagList(string label, ref List<string> list, ref Vector2 scrollPos, Color labelColor)
        {
            GUI.color = labelColor;
            GUILayout.Label(label, EditorStyles.boldLabel);
            GUI.color = Color.white;

            EditorGUILayout.BeginVertical("box");
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(80));
        
            for (int i = 0; i < list.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                list[i] = EditorGUILayout.TagField(list[i]);
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    list.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }
        
            EditorGUILayout.EndScrollView();
        
            if (GUILayout.Button("Add Tag"))
            {
                list.Add("Untagged");
            }
            EditorGUILayout.EndVertical();
        }

        // --- [Export Logic] ---

        private void Export()
        {
            string path = EditorUtility.SaveFilePanel("Save Map File", "", "MapData", "mapfile");
            if (string.IsNullOrEmpty(path)) return;

            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine("[SECTION: LAYERS]");
            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (string.IsNullOrEmpty(layerName)) continue; // 빈 레이어는 스킵

                // 1. 레이어의 충돌 마스크 계산 (나와 충돌하는 레이어들을 비트마스크로 변환)
                int collisionMask = 0;
                for (int j = 0; j < 32; j++)
                {
                    // Unity API: Ignore가 true면 충돌 안 함 -> !Ignore가 충돌 함
                    if (!Physics.GetIgnoreLayerCollision(i, j))
                    {
                        collisionMask |= (1 << j);
                    }
                }

                // 포맷: LAYER_DEF: 인덱스,이름,충돌마스크(int)
                sb.AppendLine($"LAYER_DEF: {i},{layerName},{collisionMask}");
            }
            sb.AppendLine("[SECTION: OBJECTS]");
            
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);

            int count = 0;
            
            foreach (GameObject go in allObjects)
            {
                // 0. 기본 필터 (비활성 오브젝트 제외)
                if (go.hideFlags != HideFlags.None) continue;
                if (!go.activeInHierarchy) continue;

                // --- 필터링 핵심 로직 ---

                // 1. 블랙리스트 체크 (포함 시 스킵)
                if (excludedTags.Contains(go.tag)) continue;

                // 2. 화이트리스트 체크 (포함 시 통과)
                bool isWhitelisted = includedTags.Contains(go.tag);

                // 3. 레이어 체크 (비트마스크)
                bool isLayerMatch = ((1 << go.layer) & targetLayers.value) != 0;

                // 최종 결정: 화이트리스트도 아니고, 레이어도 안 맞으면 스킵
                if (!isWhitelisted && !isLayerMatch) continue;

                // ---------------------

                // 데이터 기록
                sb.AppendLine("-");
                sb.AppendLine($"Name: {go.name}");
                sb.AppendLine($"Tag: {go.tag}");
                sb.AppendLine($"LayerIndex: {go.layer}");
                sb.AppendLine($"LayerName: {LayerMask.LayerToName(go.layer)}");

                Transform t = go.transform;
                sb.AppendLine($"Position: {FormatVec3(t.position)}");
                sb.AppendLine($"Rotation: {FormatQuat(t.rotation)}");
                sb.AppendLine($"Scale: {FormatVec3(t.lossyScale)}");

                ExportCollider(go, sb);
                ExportRigidbody(go,sb);
                ExportServerComponents(go, sb);

                count++;
            }

            File.WriteAllText(path, sb.ToString());
            Debug.Log($"Exported {count} objects to {path}");
        }


        // 유니티 네이티브 객체들을 파싱하는 함수들
        private void ExportCollider(GameObject go, StringBuilder sb)
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

            if (col.material != null)
            {
                PhysicsMaterial mat = col.material;
                sb.AppendLine("Material: " + mat.name);
                sb.AppendLine("StaticFriction: " + mat.staticFriction);
                sb.AppendLine("DynamicFriction: " + mat.dynamicFriction);
                sb.AppendLine("Bounciness: " + mat.bounciness);
                sb.AppendLine("BounceCombine: " + mat.bounceCombine);
                sb.AppendLine("FrictionCombine: " + mat.frictionCombine);
            }
            else
            {
                sb.AppendLine("Material: DefaultMaterial");
                sb.AppendLine("StaticFriction: 0.6");
                sb.AppendLine("DynamicFriction: 0.6");
                sb.AppendLine("Bounciness: 0");
                sb.AppendLine("BounceCombine: " + PhysicsMaterialCombine.Average );
                sb.AppendLine("FrictionCombine: " + PhysicsMaterialCombine.Average);
            }
        }
        private void ExportRigidbody(GameObject go, StringBuilder sb)
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

        //ServerComponent를 상속하는 서버에 넘길 컴포넌트를 파싱
        private void ExportServerComponents(GameObject go, StringBuilder sb)
        {
            ServerComponent[] serverComps = go.GetComponents<ServerComponent>();
            foreach (var comp in serverComps)
            {
                if (!comp.enabled) continue;
                sb.AppendLine($"COMPONENT: {comp.GetType().Name}");
                string data = comp.Serialize();
                if (!string.IsNullOrWhiteSpace(data)) sb.AppendLine(data);
                if(string.IsNullOrWhiteSpace(data)) Debug.LogError($"Parse Failed COMPONENT: {comp.GetType().Name}");
            }
        }

        private string FormatVec3(Vector3 v) => $"{v.x:F4},{v.y:F4},{v.z:F4}";
        private string FormatQuat(Quaternion q) => $"{q.x:F4},{q.y:F4},{q.z:F4},{q.w:F4}";

        // --- [Save/Load Settings] ---
        private void SaveSettings()
        {
            EditorPrefs.SetInt("MapExp_Layers", targetLayers.value);
            SaveList("MapExp_ExTags", excludedTags);
            SaveList("MapExp_InTags", includedTags);
        }

        private void LoadSettings()
        {
            if (EditorPrefs.HasKey("MapExp_Layers")) targetLayers.value = EditorPrefs.GetInt("MapExp_Layers");
            LoadList("MapExp_ExTags", ref excludedTags);
            LoadList("MapExp_InTags", ref includedTags);
        }

        private void SaveList(string key, List<string> list)
        {
            EditorPrefs.SetInt(key + "_Count", list.Count);
            for(int i=0; i<list.Count; i++) EditorPrefs.SetString(key + "_" + i, list[i]);
        }

        private void LoadList(string key, ref List<string> list)
        {
            if (!EditorPrefs.HasKey(key + "_Count")) return;
            list.Clear();
            int count = EditorPrefs.GetInt(key + "_Count");
            for(int i=0; i<count; i++) list.Add(EditorPrefs.GetString(key + "_" + i));
        }
    }
}
#endif