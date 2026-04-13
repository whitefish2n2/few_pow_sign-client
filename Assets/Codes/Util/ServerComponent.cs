using UnityEngine;

// 모든 맵의 서버 동기화용 컴포넌트는 이 클래스를 상속받아야 합니다.
namespace Codes.Util
{
    public abstract class ServerComponent : MonoBehaviour
    {

        public abstract string Serialize();
    }
}