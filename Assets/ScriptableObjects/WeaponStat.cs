using Codes.InGame.Weapons;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "WeaponStat", menuName = "Scriptable Object/WeaponStat", order = int.MaxValue)]
public class WeaponStat : ScriptableObject
{
    public WeaponNames weaponName;//todo:추후 weaponType enum을 만들어서 구조를 바꿔요
    public float headDamage;
    public float lagDamage;
    public float bodyDamage;
    public int maxAmmo;
    public float termToShot;
    public GameObject thirdPovObjectPrefab;
    public GameObject handleObjectPrefab;
    public AudioClip shotSound;
    public Vector3 handlePosition;
    public Vector3 handleObjectRotation;
    public Vector3 thirdPovObjectPosition;
    public Vector3 thirdPovObjectRotation;
    public WeaponType type;
    public Color interactHighlightColor;
}

public enum WeaponNames
{
    Fisher,
    Kick,
    Kcal,
}
