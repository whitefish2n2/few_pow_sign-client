using System.Collections.Generic;
using UnityEngine;

namespace Codes.Util
{
    /// <summary>
    /// 매 씬 초기화되는 instance로 전역 접근만 가능한 싱글톤의 변형 버전
    /// </summary>
    public abstract class MonoBungleton<T> : MonoBehaviour where T : MonoBungleton<T>
    {
        private static T _instance;
        private static bool _initialized;
        private static readonly object _lock = new object();

        public static T Instance
        {
            get
            {
                if (!_instance)
                    Debug.LogError($"[MonoSingleton<{typeof(T).Name}>] is not initialized!");
                return _instance;
            }
        }

        public static bool IsInitialized => _initialized;

        public static bool TryGetInstance(out T instance)
        {
            instance = _instance;
            return _initialized;
        }

        protected virtual void Awake()
        {
            lock (_lock)
            {
                if (_instance != null && _instance != this)
                {
                    Destroy(gameObject);
                    return;
                }

                _instance = (T)this;
                _initialized = true;
                Initialize();
            }
        }

        /// <summary>
        /// 초기화 코드 - 매 씬의 Awake()에서 실행
        /// </summary>
        protected abstract void Initialize();

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                _initialized = false;
            }
        }
    }
}
