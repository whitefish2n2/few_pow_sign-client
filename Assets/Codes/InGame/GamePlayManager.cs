using System.Text;
using Codes.Util;
using UnityEngine;

namespace Codes.InGame
{
    public class GamePlayManager : ServerComponent
    {
        
        public int timeoutSeconds;//서버에서 타임아웃 시간 값으로 사용함, 타임아웃 시간 내 모든 플레이어가 레디요청을 보내지 않으면 세션 종료
        public override string Serialize()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"TimeOutSeconds: {timeoutSeconds}");
            return sb.ToString();
        }
        
        
        
        
        
        
        

        //MonoBungleton
        private static GamePlayManager _instance;
        private static bool _initialized;
        private static readonly object _lock = new object();
        public static GamePlayManager Instance
        {
            get
            {
                if (!_instance)
                    Debug.LogError($"[MonoSingleton<{nameof(GamePlayManager)}>] is not initialized!");
                return _instance;
            }
        }
        public static bool IsInitialized => _initialized;

        public static bool TryGetInstance(out GamePlayManager instance)
        {
            instance = _instance;
            return _initialized;
        }

        protected void Awake()
        {
            lock (_lock)
            {
                if (_instance != null && _instance != this)
                {
                    Destroy(gameObject);
                    return;
                }

                _instance = this;
                _initialized = true;
            }
        }
        
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
