using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class TmpInputFieldInterface : MonoBehaviour
{
    [HideInInspector]public TMP_InputField tmpInputField;

    [Header("Alert Outline Config")]
    #if UNITY_EDITOR
    [SerializeField] bool seeOutlineOnInspector;
    #endif
    [SerializeField] private Color alertOutlineColor;
    [SerializeField] private Vector2 alertOutlineSize;
    [SerializeField] private bool useGraphicAlpha;
    [SerializeField] private Outline alertOutline;
    [Header("set active when trigger wrong input")]
    [SerializeField] private GameObject wrongInputComment;
    private TextMeshProUGUI wrongInputText;


    private void Reset()
    {
        
        tmpInputField = gameObject.GetComponent<TMP_InputField>();
        #if (UNITY_EDITOR)
        if (tmpInputField == null)
        {
            Debug.LogError("No TMP_InputField component found on TMP_InputFieldInterface");
            DestroyImmediate(this);
        }

        if (alertOutline)
        {
            alertOutline.effectColor = alertOutlineColor;
            alertOutline.effectDistance = alertOutlineSize;
            alertOutline.useGraphicAlpha = useGraphicAlpha;
            alertOutline.enabled = seeOutlineOnInspector;
        }
        #endif
    }

    private void OnValidate()
    {
        #if (UNITY_EDITOR)
        if (alertOutline)
        {
            alertOutline.enabled = seeOutlineOnInspector;
            alertOutline.effectColor = alertOutlineColor;
            alertOutline.effectDistance = alertOutlineSize;
            alertOutline.useGraphicAlpha = useGraphicAlpha;
        }
        #endif
    }


    private IEnumerator SubmitTermIE()
    {
        while (Input.compositionString.Length > 0)
        {
            // 아직 한글 조합 중
            yield return null;
        }
        
    } 

    private void Awake()
    {
        
        wrongInputText = wrongInputComment.GetComponent<TextMeshProUGUI>();
        if (alertOutline)
        {
            alertOutline.effectColor = alertOutlineColor;
            alertOutline.effectDistance = alertOutlineSize;
            alertOutline.useGraphicAlpha = useGraphicAlpha;
            alertOutline.enabled = false;
        }
        
        if (tmpInputField == null)
            if (!TryGetComponent<TMP_InputField>(out tmpInputField))
            {
                Debug.LogError("No TMP_InputField component found on TMP_InputFieldInterface",this);
            }
        tmpInputField.onSubmit.AddListener((string _) => {
            StartCoroutine(SubmitTermIE());
        }); 
        tmpInputField.onValueChanged.AddListener(OnTyping);
        if(alertOutline)
            alertOutline.enabled = false;
        
    }

    public void BePassword()
    {
        tmpInputField.contentType = TMP_InputField.ContentType.Password;
        tmpInputField.asteriskChar = '•';
        //문자 리로드 - 없으면 상태 갱신 안됨
        tmpInputField.ForceLabelUpdate();
        tmpInputField.textComponent.ForceMeshUpdate();
    }

    public void BeNotPassword()
    {
        tmpInputField.contentType = TMP_InputField.ContentType.Standard;
        
        //문자 리로드
        tmpInputField.ForceLabelUpdate();
        tmpInputField.textComponent.ForceMeshUpdate();
    }
    
    private bool wrongInputFlag = false;
    
    /// <summary>
    /// Trig Inner outline enabled and trigger wrongInputComment be true
    /// </summary>
    public void WrongInput()
    {
        if (alertOutline)
        {
            alertOutline.enabled = true;
        }

        if (wrongInputComment)
        {
            wrongInputComment.SetActive(true);
        }

        wrongInputFlag = true;
    }

    public void WrongInput(string text)
    {
        if (alertOutline)
        {
            alertOutline.enabled = true;
        }

        if (wrongInputComment)
        {
            wrongInputComment.SetActive(true);
            wrongInputText.text = text;
        }

        wrongInputFlag = true;
    }
    public void OnTyping(string text)
    {
        if (wrongInputFlag)
        {
            ClearWrongInput();
        }
        
    }
    
    public void ClearWrongInput()
    {
        if (alertOutline)
            alertOutline.enabled = false;

        if (wrongInputComment)
            wrongInputComment.SetActive(false);

        wrongInputFlag = false;
    }
}
