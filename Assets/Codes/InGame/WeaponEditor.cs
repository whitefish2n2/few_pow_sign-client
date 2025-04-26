using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using Codes.InGame.Weapons;

[CustomEditor(typeof(WeaponStat))]
public class WeaponEditor : Editor
{
    SerializedProperty weaponName;
    SerializedProperty headDamage;
    SerializedProperty lagDamage;
    SerializedProperty bodyDamage;
    SerializedProperty maxAmmo;
    SerializedProperty termToShot;
    SerializedProperty dropedObjectPrefab;
    SerializedProperty handledObjectPrefab;
    SerializedProperty shotSound;
    SerializedProperty handlePosition;
    SerializedProperty handleObjectRotation;
    SerializedProperty thirdPovObjectPosition;
    SerializedProperty thirdPovObjectRotation;
    SerializedProperty type;
    SerializedProperty interactHighlightColor;

    private string currentPath;
    private WeaponNames currentWeaponNames = WeaponNames.Fisher;
    private WeaponNames previousWeaponNames = WeaponNames.Fisher;
    private WeaponType currentWeaponTypes;
    private GameObject handleObjectToCopyInfo;
    private GameObject thirdPovObjectToCopyInfo;

    private void OnEnable()
    {
        weaponName = serializedObject.FindProperty("weaponName");
        headDamage = serializedObject.FindProperty("headDamage");
        lagDamage = serializedObject.FindProperty("lagDamage");
        bodyDamage = serializedObject.FindProperty("bodyDamage");
        maxAmmo = serializedObject.FindProperty("maxAmmo");
        termToShot = serializedObject.FindProperty("termToShot");
        dropedObjectPrefab = serializedObject.FindProperty("thirdPovObjectPrefab");
        handledObjectPrefab = serializedObject.FindProperty("handleObjectPrefab");
        shotSound = serializedObject.FindProperty("shotSound");
        handlePosition = serializedObject.FindProperty("handlePosition");
        handleObjectRotation = serializedObject.FindProperty("handleObjectRotation");
        thirdPovObjectPosition = serializedObject.FindProperty("thirdPovObjectPosition");
        thirdPovObjectRotation = serializedObject.FindProperty("thirdPovObjectRotation");
        type = serializedObject.FindProperty("type");
        interactHighlightColor = serializedObject.FindProperty("interactHighlightColor");

        if (serializedObject.targetObject.name != "stat") return;

        previousWeaponNames = currentWeaponNames;
        UpdateAssetPath();
    }

    private void UpdateAssetPath()
    {
        currentPath = $"Assets/ScriptableObjects/MainWeapon/{currentWeaponNames.ToString()}.asset";
        WeaponStat existingStat = AssetDatabase.LoadAssetAtPath<WeaponStat>(currentPath);
        weaponName.enumValueIndex = (int)currentWeaponNames;

        if (existingStat != null)
        {
            type.enumValueIndex = (int)existingStat.type;
            headDamage.floatValue = existingStat.headDamage;
            lagDamage.floatValue = existingStat.lagDamage;
            bodyDamage.floatValue = existingStat.bodyDamage;
            maxAmmo.intValue = existingStat.maxAmmo;
            termToShot.floatValue = existingStat.termToShot;
            dropedObjectPrefab.objectReferenceValue = existingStat.thirdPovObjectPrefab;
            handledObjectPrefab.objectReferenceValue = existingStat.handleObjectPrefab;
            shotSound.objectReferenceValue = existingStat.shotSound;
            handlePosition.vector3Value = existingStat.handlePosition;
            handleObjectRotation.vector3Value = existingStat.handleObjectRotation;
            thirdPovObjectPosition.vector3Value = existingStat.thirdPovObjectPosition;
            thirdPovObjectRotation.vector3Value = existingStat.thirdPovObjectRotation;
            interactHighlightColor.colorValue = existingStat.interactHighlightColor;
        }
        else
        {
            headDamage.floatValue = 0;
            lagDamage.floatValue = 0;
            bodyDamage.floatValue = 0;
            maxAmmo.intValue = 0;
            termToShot.floatValue = 0;
            dropedObjectPrefab.objectReferenceValue = null;
            handledObjectPrefab.objectReferenceValue = null;
            shotSound.objectReferenceValue = null;
            handlePosition.vector3Value = Vector3.zero;
            handleObjectRotation.vector3Value = Vector3.zero;
            thirdPovObjectPosition.vector3Value = Vector3.zero;
            thirdPovObjectRotation.vector3Value = Vector3.zero;
            interactHighlightColor.colorValue = Color.clear;
        }

        serializedObject.ApplyModifiedProperties();
        serializedObject.Update();
    }

    public override void OnInspectorGUI()
    {
        if (serializedObject.targetObject.name != "stat")
        {
            EditorGUILayout.HelpBox("stat에서 관리하셈", MessageType.Warning);
            DrawDefaultInspector();
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
            return;
        }

        serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        currentWeaponNames = (WeaponNames)EditorGUILayout.EnumPopup("Weapon Name", currentWeaponNames);

        // 변경된 경우에만 로드
        if (currentWeaponNames != previousWeaponNames)
        {
            previousWeaponNames = currentWeaponNames;
            UpdateAssetPath();
        }

        // ScriptableObject 없을 경우 생성
        if (!File.Exists(currentPath))
        {
            if (GUILayout.Button($"Create {currentWeaponNames} ScriptableObject"))
            {
                WeaponStat newStat = CreateInstance<WeaponStat>();
                newStat.weaponName = currentWeaponNames;
                newStat.type = (WeaponType)type.enumValueIndex;

                string directoryPath = Path.GetDirectoryName(currentPath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                AssetDatabase.CreateAsset(newStat, currentPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        else
        {
            EditorGUILayout.LabelField($"{currentWeaponNames} ScriptableObject exists.");
        }

        currentWeaponTypes = (WeaponType)EditorGUILayout.EnumPopup("Weapon type", currentWeaponTypes);

        EditorGUILayout.PropertyField(headDamage);
        EditorGUILayout.PropertyField(lagDamage);
        EditorGUILayout.PropertyField(bodyDamage);
        EditorGUILayout.PropertyField(maxAmmo);
        EditorGUILayout.PropertyField(termToShot);
        EditorGUILayout.PropertyField(dropedObjectPrefab);
        EditorGUILayout.PropertyField(handledObjectPrefab);
        EditorGUILayout.PropertyField(shotSound);
        EditorGUILayout.PropertyField(handlePosition);
        EditorGUILayout.PropertyField(handleObjectRotation);
        handleObjectToCopyInfo = (GameObject)EditorGUILayout.ObjectField("Handle ref", handleObjectToCopyInfo, typeof(GameObject), true);
        if (GUILayout.Button($"Copy handlePos/Rot of this object"))
        {
            handlePosition.vector3Value = handleObjectToCopyInfo.transform.localPosition;
            handleObjectRotation.vector3Value = handleObjectToCopyInfo.transform.localRotation.eulerAngles;
        }
        EditorGUILayout.PropertyField(thirdPovObjectPosition);
        EditorGUILayout.PropertyField(thirdPovObjectRotation);
        thirdPovObjectToCopyInfo = (GameObject)EditorGUILayout.ObjectField("ThirdPov ref", thirdPovObjectToCopyInfo, typeof(GameObject), true);
        if (GUILayout.Button($"Copy thirdPovPos/Rot of this object"))
        {
            thirdPovObjectPosition.vector3Value = handleObjectToCopyInfo.transform.localPosition;
            thirdPovObjectRotation.vector3Value = handleObjectToCopyInfo.transform.localRotation.eulerAngles;
        }
        EditorGUILayout.PropertyField(interactHighlightColor);

        if (EditorGUI.EndChangeCheck())
        {
            type.enumValueIndex = (int)currentWeaponTypes;
            OnPropertyChanged();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void OnPropertyChanged()
    {
        WeaponStat existingStat = AssetDatabase.LoadAssetAtPath<WeaponStat>(currentPath);
        if (existingStat != null)
        {
            existingStat.type = currentWeaponTypes;
            existingStat.weaponName = (WeaponNames)weaponName.enumValueIndex;
            existingStat.headDamage = headDamage.floatValue;
            existingStat.lagDamage = lagDamage.floatValue;
            existingStat.bodyDamage = bodyDamage.floatValue;
            existingStat.maxAmmo = maxAmmo.intValue;
            existingStat.termToShot = termToShot.floatValue;
            existingStat.thirdPovObjectPrefab = dropedObjectPrefab.objectReferenceValue as GameObject;
            existingStat.handleObjectPrefab = handledObjectPrefab.objectReferenceValue as GameObject;
            existingStat.shotSound = shotSound.objectReferenceValue as AudioClip;
            existingStat.handlePosition = handlePosition.vector3Value;
            existingStat.handleObjectRotation = handleObjectRotation.vector3Value;
            existingStat.thirdPovObjectPosition = thirdPovObjectPosition.vector3Value;
            existingStat.thirdPovObjectRotation = thirdPovObjectRotation.vector3Value;
            existingStat.interactHighlightColor = interactHighlightColor.colorValue;

            EditorUtility.SetDirty(existingStat);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    static string GetCurrentFileName([CallerFilePath] string filePath = "")
    {
        return Path.GetFileName(filePath);
    }
}




