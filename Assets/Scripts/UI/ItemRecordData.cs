using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using Assets.Scripts.Records;

namespace Assets.Scripts.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ItemRecordData : UIBehaviour
    {
        [SerializeField] TextMeshProUGUI m_TimeLabel;
        [SerializeField] TextMeshProUGUI m_MovesLabel;
        private CanvasGroup mCanvasGroup;

        protected override void Awake()
        {
            base.Awake();
            mCanvasGroup = GetComponent<CanvasGroup>();
        }

        public void SetData(RecordData recordData)
        {
            if (m_TimeLabel) m_TimeLabel.text = recordData.RecordTime.ToString(@"hh\:mm\:ss");
            if (m_MovesLabel) m_MovesLabel.text = recordData.MovesCount.ToString();
            if (mCanvasGroup)
                if (recordData.Sum == 0) mCanvasGroup.alpha = .5f;
                else mCanvasGroup.alpha = 1f;
        }
    }
}
