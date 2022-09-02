using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static Assets.Scripts.GameControllers.GameController;

namespace Assets.Scripts.AnimControllers
{
    class GameAnimController : AnimController
    {
        [SerializeField] private TextMeshProUGUI m_MessageTxt = null;
        [SerializeField] private Controllers m_Controller = Controllers.Pause;
        [SerializeField] private UnityEvent<Controllers> m_OnShow = null;
        [SerializeField] private UnityEvent<Controllers> m_OnHide = null;

        public UnityEvent<Controllers> OnShow { get => m_OnShow; set => m_OnShow = value; }
        public UnityEvent<Controllers> OnHide { get => m_OnHide; set => m_OnHide = value; }

        private void Show() => m_OnShow.Invoke(m_Controller);
        private void Hide() => m_OnHide.Invoke(m_Controller);

        public void ShowIs(bool value) => SetParam(0, value);

        public void SetMessage(string message) => m_MessageTxt.text = message;
        public void SetMessage(string message, Color messageColor)
        {
            SetMessage(message);
            m_MessageTxt.color = messageColor;
        }
    }
}