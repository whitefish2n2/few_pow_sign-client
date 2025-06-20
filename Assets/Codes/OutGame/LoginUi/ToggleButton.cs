using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


/// <summary>
/// button, image 스크립트에 의존하는 클래스에요
/// 클릭할때 토글로 버튼 상태, 스프라이트를 변경하고 각각 등록한 UnityEvent를 실행해요
/// Start에 startstate에 따른 이벤트를 한번 실행해요
/// </summary>
public class ToggleButton : MonoBehaviour
{
    [SerializeField] private bool startState = false;
    [SerializeField] private UnityEvent onToggleFalse;
    [SerializeField] private UnityEvent onToggleTrue;
    [SerializeField] private Sprite falseSprite;
    [SerializeField] private Sprite trueSprite;
    [HideInInspector]public bool currentState;
    private Image image;
    private Button button;

    private void Reset()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();
        if (!image || !button)
        {
            Debug.LogError("ToggleButton depends on button script!" );
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null)
                    DestroyImmediate(this);
            };
#endif
        }
        if (image == null) return;
        image.sprite = startState ? trueSprite : falseSprite;
        
    }
    private void OnValidate()
    {
        if(!image)
            TryGetComponent(out image); 
        
        image.sprite = startState? trueSprite: falseSprite;
    }

    
    private void Click()
    {
        
        if(currentState)
            onToggleFalse.Invoke();
        else
            onToggleTrue.Invoke();
        currentState = !currentState;
        image.sprite = currentState ? trueSprite : falseSprite;
    }

    private void Start()
    {
        currentState = startState;
        if(!button)
            TryGetComponent(out button);
        if (currentState)
            onToggleTrue.Invoke();
        else
            onToggleFalse.Invoke();
        button.onClick.AddListener(Click);
    }
}
