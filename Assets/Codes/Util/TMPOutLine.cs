using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TMPOutLine : MonoBehaviour
{
    private TextMeshProUGUI textMeshProUGUI;
    [SerializeField] private Color color;
    [Range(0,1)] [SerializeField] private float width;

    private void Awake()
    {
        if (!textMeshProUGUI)
            TryGetComponent(out textMeshProUGUI);
    }

    private void Start()
    {
        StartCoroutine(ApplyOutlineDelayed());
    }

    // Awake 시점엔 폰트 에셋(특히 큰 아틀라스)이 완전히 준비 안 됐을 수 있어서
    // 한 프레임 미루고 outline 적용 + 강제 메시 갱신
    private IEnumerator ApplyOutlineDelayed()
    {
        yield return null;

        if (!textMeshProUGUI || !textMeshProUGUI.fontSharedMaterial)
        {
            Debug.LogError("TMPOutLine : No TMPOutLine component attached");
            yield break;
        }
        textMeshProUGUI.outlineColor = color;
        textMeshProUGUI.outlineWidth = width;
        textMeshProUGUI.ForceMeshUpdate();
    }

    private void Reset()
    {
        if (!TryGetComponent<TextMeshProUGUI>(out var component))
        {
            Debug.LogWarning("TMPOutline component requires a TextMeshProUGUI. Removing self.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null)
                    DestroyImmediate(this);
            };
#endif
        }

        if (component == null) return;
        textMeshProUGUI = component;
        textMeshProUGUI.outlineColor = color;
        textMeshProUGUI.outlineWidth = width;
    }
    
    private void OnValidate()
    {
        if (!textMeshProUGUI)
        {
            TryGetComponent(out textMeshProUGUI);
        }

        if (!textMeshProUGUI || !textMeshProUGUI.fontSharedMaterial)
        {
            return;
        }

        textMeshProUGUI.outlineColor = color;
        textMeshProUGUI.outlineWidth= width;
    }
}
