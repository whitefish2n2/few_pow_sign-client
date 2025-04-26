using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

namespace Codes.InGame
{
    public class GameUtil : MonoBehaviour
    {
        public static GameUtil instance;
        void Start()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void CoolBool(float t, Action<bool> callback, bool to = true)
        {
            StartCoroutine(CoolBoolIE(t, callback, to));
        }
        private IEnumerator CoolBoolIE(float t, Action<bool> target, bool to = true)
        {
            target(!to);
            yield return new WaitForSeconds(t);
            target(to);
        }

        public void SlowMotion(float t,float timeScaleOnSlow,float timeScaleOnEnd = 1f,  Action callback = null)
        {
            StartCoroutine(SlowMotionIE(t,timeScaleOnSlow, timeScaleOnEnd, callback));
        }

        IEnumerator SlowMotionIE(float t, float timeScaleOnSlow, float timeScaleOnEnd, [CanBeNull] Action callback = null)
        {
            Time.timeScale = timeScaleOnSlow;
            yield return new WaitForSecondsRealtime(t);
            Time.timeScale = timeScaleOnEnd;
            callback?.Invoke();
            var a = Physics2D.BoxCastAll(Vector3.up, Vector2.zero, 3, Vector2.zero);
        }
    }
}
