using TMPro;
using UnityEngine;

namespace NetTest
{
    public class ModalManager : MonoBehaviour
    {
        public static ModalManager instance;
        private TextMeshProUGUI t;
        [SerializeField] private GameObject modal;
        private void Awake()
        {
            instance = this;
            t = modal.GetComponentInChildren<TextMeshProUGUI>();
        }
    
        public void Alert(string text)
        {
            t.text = text;
        }
    
    }
}
