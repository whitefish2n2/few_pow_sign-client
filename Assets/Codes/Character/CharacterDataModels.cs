using System;
using System.Collections.Generic;
namespace Codes.Character
{


    public enum CharacterRole
    {
        attack,
        defense,
        support,
    }
    public static class CharacterRoleExtensions
    {
        // 1. Enum을 String으로 변환 (UI 출력용)
        public static string ToDisplayString(this CharacterRole role)
        {
            switch (role)
            {
                case CharacterRole.attack: return "Attack";
                case CharacterRole.defense: return "Defense";
                case CharacterRole.support: return "Support";
                default: return "Unknown";
            }
        }

        // 2. String을 Enum으로 변환 (데이터 파싱용)
        public static CharacterRole ToRoleEnum(string roleString)
        {
            if (string.IsNullOrEmpty(roleString)) return CharacterRole.attack;

            switch (roleString.ToLower())
            {
                case "attack": return CharacterRole.attack;
                case "defense": 
                case "defence":
                    return CharacterRole.defense;
                case "support": return CharacterRole.support;
                default: 
                    UnityEngine.Debug.LogWarning($"알 수 없는 역할군 문자열: {roleString}");
                    return CharacterRole.attack;
            }
        }
    }
    [Serializable]
    public class CharacterDatabase
    {
        public string dataVersion;
        public List<CharacterData> characterList;
    }

    [Serializable]
    public class CharacterData
    {
        public int id;
        public string characterId;
        public string role;
        public int prefabId;
        public CharacterStats baseStats;
    }

    [Serializable]
    public class CharacterStats
    {
        public int maxHp;
        public int speed;
    }
}