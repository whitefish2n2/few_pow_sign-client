using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeaponDatabase
{
    public List<WeaponData> weaponList;
}

[Serializable]
public class WeaponData
{
    public int id;
    public string weaponName; 
    public string type;         // WeaponType
    public float  headDamage;
    public float  lagDamage;
    public float  bodyDamage;
    public int    maxAmmo;
    public float  termToShot;
    public Vector3 handlePosition;
    public Vector3 handleObjectRotation;
    public Vector3 thirdPovObjectPosition;
    public Vector3 thirdPovObjectRotation;
    public Color   interactHighlightColor;
    public string  handleObjectPrefabName;    // 에셋참조 → 이름
    public string  thirdPovObjectPrefabName;
    public string  shotSoundName;
}