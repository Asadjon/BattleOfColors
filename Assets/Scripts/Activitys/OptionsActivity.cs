using Assets.Scripts.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Activitys
{
    public class OptionsActivity : Activity
    {
        public static int ActivityType { get; set; }
        
        [SerializeField] private NumberPicker m_NumberPicker = null;
        [SerializeField] private ToggleGroup m_GameTypes = null;
        [SerializeField] private ToggleGroup m_GameLevels = null;

        protected override void Awake()
        {
            base.Awake();

            var gameOptions = GameOptions.Instance;

            var disVal = new List<string>();
            for (int i = GameOptions.MinNumberOfArrays; i <= GameOptions.MaxNumberOfArrays; i++)
                disVal.Add(i.ToString());

            m_NumberPicker.DisplayedValues = disVal;
            m_NumberPicker.Value = disVal.IndexOf(gameOptions.NumberOfArrays.ToString());
            m_NumberPicker.OnChangeValue.AddListener(value =>
            gameOptions.NumberOfArrays = int.Parse(disVal[value]));

            var gameTypes = m_GameTypes.GetComponentsInChildren<Toggle>();
            if (gameTypes.Length - 1 == (int)GameOptions.GameTypes.WithNumber)
            {
                foreach (var type in gameTypes)
                    type.onValueChanged.AddListener(value =>
                    { if (value) gameOptions.GameType = (GameOptions.GameTypes)Array.IndexOf(gameTypes, type); });
                gameTypes[(int)gameOptions.GameType].isOn = true;
            }


            var gameLevels = m_GameLevels.GetComponentsInChildren<Toggle>();
            if (gameLevels.Length - 1 < (int)GameOptions.GameLevels.Expert) return;

            gameLevels[(int)gameOptions.Level].isOn = true;
            foreach (var level in gameLevels)
                level.onValueChanged.AddListener(value =>
                { if (value) gameOptions.Level = (GameOptions.GameLevels)Array.IndexOf(gameLevels, level); });
        }

        public void PlaySound(string soundName) => AudioManager.Instance.Play(soundName);

        public void StartGameClick() =>
            StartTransitionAnim(ActivityType);

        #region Activites actions

        public override void OnBackPressed() =>
            StartTransitionAnim(ActivitesID.Instance.GetId<MenuActivity>());

        public override void StartActivity() =>
            Screen.orientation = ScreenOrientation.Portrait;

        public override void WaitActivity() => Finish();

        #endregion
    }
}
