using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Activitys
{
    class RecordsActivity : Activity
    {
        [SerializeField] private Transform m_WithNumbers;
        [SerializeField] private Transform m_WithColors;

        private readonly List<TMPro.TextMeshProUGUI> mNumbers = new List<TMPro.TextMeshProUGUI>();
        private readonly List<TMPro.TextMeshProUGUI> mColors = new List<TMPro.TextMeshProUGUI>();

        [ContextMenu("Init")]
        public void Init()
        {
            mNumbers.Clear();
            mColors.Clear();

            mNumbers.AddRange(m_WithNumbers.GetComponentsInChildren<TMPro.TextMeshProUGUI>());
            mColors.AddRange(m_WithColors.GetComponentsInChildren<TMPro.TextMeshProUGUI>());
        }

        protected override void Awake()
        {
            base.Awake();
            Init();

            mNumbers.ForEach(num =>
            {
                var rec = RecordHelper.GetRecord(mNumbers.IndexOf(num) + 3, GameOptions.GameTypes.WithNumber);
                num.text = rec.numberOfArrays + " arrays: " + ((TimeSpan) rec.time).ToString(@"hh\:mm\:ss");
            });

            mColors.ForEach(col =>
            {
                var rec = RecordHelper.GetRecord(mColors.IndexOf(col) + 3, GameOptions.GameTypes.WithColor);
                col.text = rec.numberOfArrays + " arrays: " + ((TimeSpan)rec.time).ToString(@"hh\:mm\:ss");
            });
        }
        public void PlaySound(string soundName) => AudioManager.Instance.Play(soundName);

        #region Activites actions

        public override void OnBackPressed() =>
            StartTransitionAnim(ActivitesID.Instance.GetId<MenuActivity>());

        public override void StartActivity() =>
            Screen.orientation = ScreenOrientation.Portrait;

        public override void WaitActivity() => Finish();

        #endregion
    }
}