using UnityEngine;

namespace Codes.Util
{
    // 콜라이더 달린 어떤 오브젝트든 원점을 발로 맞추는 공용 유틸 (플레이어 전용 아님)
    public static class FootPivot
    {
        // 원점 → 콜라이더 최하단(발) 거리 (양수)
        public static float GetFootOffset(GameObject go)
        {
            var cols = go.GetComponentsInChildren<Collider>();
            if (cols.Length == 0) return 0f;
            Bounds b = cols[0].bounds;
            for (int i = 1; i < cols.Length; i++) b.Encapsulate(cols[i].bounds);
            return go.transform.position.y - b.min.y;
        }

        // 콜라이더 center + 자식을 footOffset만큼 올려, transform 원점이 발에 오게 함.
        // Instantiate 직후(컨트롤 AddComponent 전, 위치 그대로)에 호출.
        public static void MoveCenterToFoot(GameObject go)
        {
            float foot = GetFootOffset(go);
            if (foot <= 0f) return;
            Vector3 up = new Vector3(0f, foot, 0f);

            foreach (Transform child in go.transform)
                child.localPosition += up;          // 메쉬/앵커 자식

            foreach (var col in go.GetComponents<Collider>())
            {
                switch (col)
                {
                    case CapsuleCollider c: c.center += up; break;
                    case SphereCollider s:  s.center += up; break;
                    case BoxCollider bx:    bx.center += up; break;
                }
            }
        }
    }
}