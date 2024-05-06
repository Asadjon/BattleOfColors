using UnityEngine;
using Assets.Scripts.UI;
using Assets.Scripts.Custom;
using static Assets.Scripts.GameOptions;
using Assets.Scripts.Records;
using UnityEngine.UI;
using Assets.Scripts.AudioManagers;

namespace Assets.Scripts.Activitys
{
    class RecordsActivity : Activity
    {
        [SerializeField] private ToggleGroupForEnum<GameTypes> m_GameTypes;
        [SerializeField] private ToggleGroupForEnum<GameLevels> m_GameLevels;
        [SerializeField] private SerializableDictionary<SizesOfSquare, ItemRecordData> m_ItemRecords;
        [SerializeField] private Button m_BackButton;
        [SerializeField] private string m_ButtonSounName;

        private RecordController mRecordController;
        private GameTypes mSelectedGameType = DefaultGameType;
        private GameLevels mSelectedGameLevel = DefaultGameLevel;

        public override void OnCreate(Bundle bundle)
        {
            mRecordController = RecordController.Instance;
        }

        protected override void Start()
        {
            base.Start();

            { if (m_GameTypes.Toggles.TryGetValue(mSelectedGameType, out ToggleForEnum<GameTypes> toggle) && toggle) toggle.isOn = true; }
            { if (m_GameLevels.Toggles.TryGetValue(mSelectedGameLevel, out ToggleForEnum<GameLevels> toggle) && toggle) toggle.isOn = true; }

            m_GameTypes.OnChangeEnumValue.AddListener(gameType =>
            {
                mSelectedGameType = gameType;
                ShowData();
                PlaySound(m_ButtonSounName);
            });

            m_GameLevels.OnChangeEnumValue.AddListener(gameLevel =>
            {
                mSelectedGameLevel = gameLevel;
                ShowData();
                PlaySound(m_ButtonSounName);
            });

            m_BackButton.onClick.AddListener(() => {
                OnBackPressed();
                PlaySound(m_ButtonSounName);
            });

            ShowData();
        }

        private void ShowData()
        {
            foreach (SizesOfSquare size in typeof(SizesOfSquare).GetEnumValues())
                if (m_ItemRecords.TryGetValue(size, out ItemRecordData itemRecord) && itemRecord)
                    itemRecord.SetData(mRecordController[mSelectedGameType][size][mSelectedGameLevel]);
        }

        public void PlaySound(string soundName) => AudioManager.Instance.Play(soundName);

        #region Activites actions

        //public override void OnBackPressed() =>
        //    StartActivity<MenuActivity>();

        public override void OnPlay() =>
            Screen.orientation = ScreenOrientation.Portrait;

        //public override void OnPause() => Finish();

        #endregion
    }
}