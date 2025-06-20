using System;
using Codes.OutGame.Modal;
using NetTest;
using Plugins;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// ArertCanvas에 들어가는 MonoSingleton
/// </summary>
public class AlertManager : MonoSingleton<AlertManager>
{
    [SerializeField] private GameObject retryableErrorModal;
    [SerializeField] private GameObject defaultErrorModal;
    [SerializeField] private GameObject defaultAlertModal;

    protected override void Initialize()
    { }

    public void AlertError(ErrorResponse response)
    {
        var modal = Instantiate(defaultErrorModal, gameObject.transform, false);
        DefaultErrorModal modalCode = modal.GetComponent<DefaultErrorModal>();;
        modalCode.Alert(response);
    }
    
    public void AlertRetryableError(ErrorResponse response, Action onAlertClose = null)
    {
        var modal = Instantiate(retryableErrorModal,gameObject.transform, false);
        RetryableErrorModal retryableErrorModalCode = modal.GetComponent<RetryableErrorModal>();;
        retryableErrorModalCode.Alert(response, onAlertClose);
    }

    public void Alert(string message, Action onClose = null)
    {
        
    }
    
}
