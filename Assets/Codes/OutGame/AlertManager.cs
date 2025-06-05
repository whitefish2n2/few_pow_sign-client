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

    protected override void Initialize()
    { }

    public void AlertError(ErrorResponse response, Action onRetry)
    {
        if (!defaultErrorModal) return;
        var modal = Instantiate(defaultErrorModal);
        RetryableErrorModal retryableErrorModalCode = modal.GetComponent<RetryableErrorModal>();;
        retryableErrorModalCode.Alert(response, onRetry);
    }
    
    public void AlertRetryableError(ErrorResponse response, Action onRetry)
    {
        if (!retryableErrorModal) return;
        var modal = Instantiate(retryableErrorModal);
        RetryableErrorModal retryableErrorModalCode = modal.GetComponent<RetryableErrorModal>();;
        retryableErrorModalCode.Alert(response, onRetry);
    }
    
}
