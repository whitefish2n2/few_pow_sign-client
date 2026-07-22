#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class WeaponDataExporter
{
    private const string OutputPath = "Assets/WeaponData.json";

    [MenuItem("Tools/Export WeaponData JSON")]
    public static void Export()
    {
        var db = new WeaponDatabase { weaponList = new List<WeaponData>() };

        // 프로젝트 내 모든 WeaponStat SO 수집
        foreach (var guid in AssetDatabase.FindAssets("t:WeaponStat"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            WeaponStat stat = AssetDatabase.LoadAssetAtPath<WeaponStat>(path);
            if (stat == null) continue;
            if (stat.name == "stat") continue;

            db.weaponList.Add(new WeaponData
            {
                id = (int)stat.weaponName,
                weaponName               = stat.weaponName.ToString(),
                type                     = stat.type.ToString(),
                headDamage               = stat.headDamage,
                lagDamage                = stat.lagDamage,
                bodyDamage               = stat.bodyDamage,
                maxAmmo                  = stat.maxAmmo,
                termToShot               = stat.termToShot,
                handlePosition           = stat.handlePosition,
                handleObjectRotation     = stat.handleObjectRotation,
                thirdPovObjectPosition   = stat.thirdPovObjectPosition,
                thirdPovObjectRotation   = stat.thirdPovObjectRotation,
                interactHighlightColor   = stat.interactHighlightColor,
                handleObjectPrefabName   = stat.handleObjectPrefab   ? stat.handleObjectPrefab.name   : "",
                thirdPovObjectPrefabName = stat.thirdPovObjectPrefab ? stat.thirdPovObjectPrefab.name : "",
                shotSoundName            = stat.shotSound            ? stat.shotSound.name            : "",
            });
        }

        File.WriteAllText(OutputPath, JsonUtility.ToJson(db, true));
        AssetDatabase.Refresh();
        Debug.Log($"WeaponData export 완료: {db.weaponList.Count}개 → {OutputPath}");
    }
}
#endif