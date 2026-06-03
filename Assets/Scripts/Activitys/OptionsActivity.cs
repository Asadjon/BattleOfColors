using Assets.Scripts.UI;
using System;
using UnityEngine;
using static Assets.Scripts.GameOptions;

namespace Assets.Scripts.Activitys
{
    public class OptionsActivity : Activity
    {
        public const string ACTIVITY_TYPE = "ACTIVITY_TYPE";

        [SerializeField] private ToggleGroupForEnum<SizesOfSquare> m_SizesOfSquare;
        [SerializeField] private ToggleGroupForEnum<GameTypes> m_GameTypes;
        [SerializeField] private ToggleGroupForEnum<GameLevels> m_GameLevels;

        private GameOptions mGameOptions;
        private Type mStartingActivity;

        public override void OnCreate(Bundle bundle)
        {
            if (bundle != null && bundle.ContainsKey(ACTIVITY_TYPE))
                mStartingActivity = (Type)bundle[ACTIVITY_TYPE];
            mGameOptions = GameOptions.Instance;
        }

        public override void OnPlay()
        {
            Screen.orientation = ScreenOrientation.Portrait;
            base.OnPlay();
        }

        protected override void Start()
        {
            base.Awake();

            { if (m_SizesOfSquare.Toggles.TryGetValue(mGameOptions.SizeOfSquar, out ToggleForEnum<SizesOfSquare> toggle) && toggle) toggle.isOn = true; }
            { if (m_GameTypes.Toggles.TryGetValue(mGameOptions.GameType, out ToggleForEnum<GameTypes> toggle) && toggle) toggle.isOn = true; }
            { if (m_GameLevels.Toggles.TryGetValue(mGameOptions.GameLevel, out ToggleForEnum<GameLevels> toggle) && toggle) toggle.isOn = true; }

            m_SizesOfSquare.OnChangeEnumValue.AddListener(selectedSizeOfSquare =>
                mGameOptions.SizeOfSquar = selectedSizeOfSquare);

            m_GameTypes.OnChangeEnumValue.AddListener(selectedGameType =>
                mGameOptions.GameType = selectedGameType);

            m_GameLevels.OnChangeEnumValue.AddListener(selectedLevel =>
                mGameOptions.GameLevel = selectedLevel);
        }

        public void OnStartActivity_Click() =>
            StartActivity(mStartingActivity);
    }
}
