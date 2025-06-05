using System.Threading.Tasks;
using DG.Tweening;
using Plugins;
using UnityEngine;

namespace Codes.OutGame
{
    /// <summary>EventCanvas(Canvas)에 들어가는 모노싱글톤 </summary>
    public class UIEventManager : MonoSingleton<UIEventManager>
    {

        [SerializeField] private GameObject fadeBackground;
        private CanvasGroup fadeCanvasGroup;
    
        [SerializeField] private GameObject spinnerBackground;
        private CanvasGroup spinnerCanvasGroup;
        [SerializeField] private GameObject spinnerObject;
        private Animator spinnerAnimator;
    
        [SerializeField] private GameObject loadBackground;
        private Animator loadAnimator;
    
        protected override void Initialize()
        {
            fadeCanvasGroup = fadeBackground.GetComponent<CanvasGroup>();
            spinnerCanvasGroup = spinnerObject.GetComponent<CanvasGroup>();
            spinnerAnimator = spinnerObject.GetComponent<Animator>();
            //loadAnimator= loadBackground.GetComponent<Animator>();
        }

        public void FadeIn(float insert)
        {
            fadeBackground.SetActive(true);
            fadeCanvasGroup.DOFade(1, insert);
        }

        public void FadeOut(float insert)
        {
            fadeCanvasGroup.DOFade(0, insert)
                .OnComplete(()=> fadeBackground.SetActive(false));
        }

        public void DoFade(float insert, float wait, float outsert)
        {
            Sequence seq = DOTween.Sequence();
            seq.OnPlay(()=>fadeBackground.SetActive(true))
                .Append(fadeCanvasGroup.DOFade(1, insert))
                .AppendInterval(wait)
                .Append(fadeCanvasGroup.DOFade(0, outsert))
                .OnComplete(() => fadeBackground.SetActive(false));
            seq.Play();
        }

        public void SpinningIn(float insert)
        {
            spinnerBackground.SetActive(true);
            spinnerCanvasGroup.DOFade(1, insert);
        }

        public void SpinningOut(float insert)
        {
            spinnerCanvasGroup.DOFade(0, insert)
                .OnComplete(() => spinnerBackground.SetActive(false));
        }

        public async Task SpinningInAsync(float insert, Task task, float outsert)
        {
            SpinningIn(insert);
            await task;
            SpinningOut(outsert);
        }
    }
}
