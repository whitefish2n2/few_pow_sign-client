using Codes.Util;
using UnityEngine;

namespace Codes.InGame
{
    public class SoundManager : MonoBungleton<SoundManager>
    {
        [SerializeField] private AudioSource sfx2DSource;   // 위치 없이 본인만 듣는 사운드용(피격 확인음 등)
        [SerializeField] private AudioClip hitConfirmClip;

        protected override void Initialize() { }

        // 위치가 있는 3D 사운드(총성 등) — 그 자리에 임시 AudioSource를 만들어 한 번 재생 후 자동 정리됨
        public void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null) return;
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }

        // 위치 없는 2D 사운드
        public void PlaySound2D(AudioClip clip, float volume = 1f)
        {
            if (clip == null || sfx2DSource == null) return;
            sfx2DSource.PlayOneShot(clip, volume);
        }

        // 내가 쏜 총이 맞았을 때 나만 듣는 히트마커 사운드
        public void PlayHitConfirm()
        {
            PlaySound2D(hitConfirmClip);
        }
    }
}
