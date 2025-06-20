using System;
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
        if (!textMeshProUGUI || !textMeshProUGUI.fontSharedMaterial)
        {
            Debug.LogError("TMPOutLine : No TMPOutLine component attached");
            return;
        }
        textMeshProUGUI.outlineColor = color;
        textMeshProUGUI.outlineWidth= width;
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
