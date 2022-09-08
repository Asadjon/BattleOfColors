 using System;
using UnityEngine;
using TMPro;
using Assets.Scripts.Players;
using Assets.Scripts.Resource;
using Assets.Scripts.UI;
using Assets.Scripts.GameControllers;
using Assets.Scripts.SaveGameDatas;

using static Assets.Scripts.GameControllers.GameController;
using static Assets.Scripts.Activitys.SinglePlayerGameActivity;
using Assets.Scripts.SaveGameDatas.Attributes;

namespace Assets.Scripts.Activitys
{
    [Serialization(typeof(SerializationGame))]
    class SinglePlayerGameActivity : Activity, IOnGameActions
    {
        [SerializeField] private Player m_Player;
        [SerializeField] private GameController m_GameController = null;
        [SerializeField] private SecundamerView m_SecundamerView = null;
        [SerializeField] private TextMeshProUGUI m_TextForRecordTime = null;
        [SerializeField] private Gradient m_SecundamerGradient = null;
        [SerializeField] private string m_PushSoundName = default;

        private int GameType { get => (int)GameOptions.Instance.GameType; set
            {
                GameOptions.Instance.GameType = (GameOptions.GameTypes)value;
                m_Player.GameType = GameOptions.Instance.GameType;
            }
        }
        private int NumberOfArrays { get => GameOptions.Instance.NumberOfArrays; set
            {
                GameOptions.Instance.NumberOfArrays = value;
                mRecordTime = RecordHelper.GetRecord(value, GameOptions.Instance.GameType).time;
                m_Player.InitializeGame(value, ViewResource.GenerateResources(value));
            }
        }
        private double TotalMillSec { get => m_SecundamerView.Value; set => m_SecundamerView.Value = value; }

        private TimeSpan mRecordTime;

        private const string savedDataFileName = "PlayGame.dat";

        protected override void Start()
        {
            base.Start();
            LoadData();
        }

        private void LoadData()
        {
            OptionsActivity.ActivityType = SceneId;
            var gameOptions = GameOptions.Instance;

            m_Player.OnGameOver.AddListener(GameOver);
            m_Player.AddSwipeAction(() => PlaySound(m_PushSoundName));

            mRecordTime = RecordHelper.GetRecord(gameOptions.NumberOfArrays, gameOptions.GameType).time;
            m_TextForRecordTime.text = "Record: " + mRecordTime.ToString(@"hh\:mm\:ss");

            m_SecundamerView.OnValueChange.AddListener(time =>
                m_SecundamerView.ChangeTextColor(
                    m_SecundamerGradient.Evaluate(
                        Mathf.Clamp(mRecordTime.TotalMilliseconds != 0 ? (float)(time.TotalMilliseconds / mRecordTime.TotalMilliseconds) : 0, 0, 1))));

            m_GameController.SetGameActions(this);

            if (GetSavedData()) PauseGame();
            else
            {
                m_Player.GameType = gameOptions.GameType;
                m_Player.InitializeGame(gameOptions.NumberOfArrays,
                    ViewResource.GenerateResources(gameOptions.NumberOfArrays));

                m_SecundamerView.ResetTime();

                m_GameController.StartGame();
            }
        }
        public void PlaySound(string soundName) => AudioManager.Instance.Play(soundName);

        private void StartGame()
        {
            m_SecundamerView.StartTime();
            m_Player.StartGame();
        }

        private void GameOver(string message)
        {
            m_SecundamerView.StopTime();

            message = "Time\n" + m_SecundamerView.ToString();
            var messageColor = Color.white;

            if (m_SecundamerView.CurrentTime.TotalMilliseconds < mRecordTime.TotalMilliseconds || mRecordTime.TotalMilliseconds == 0)
            {
                mRecordTime = m_SecundamerView.CurrentTime;
                RecordHelper.SaveRecord(GameOptions.Instance.NumberOfArrays,
                new GameTime { hour = mRecordTime.Hours, minute = mRecordTime.Minutes, second = mRecordTime.Seconds },
                GameOptions.Instance.GameType);

                message = "New record\n" + m_SecundamerView.ToString();
                messageColor = Color.green;
            }

            m_GameController.SetMessage(message, messageColor);
            m_GameController.GameOver();
            m_Player.PauseGame();
        }

        public void PauseGameForSettings()
        {
            m_SecundamerView.StopTime();
            m_Player.PauseGame();
        }

        public void PauseGame()
        {
            PauseGameForSettings();
            m_GameController.PauseGame();
        }

        private void NewGame()
        {
            m_SecundamerView.ChangeTextColor(m_SecundamerGradient.Evaluate(0));
            m_SecundamerView.ResetTime();
            m_Player.NewGame();
        }

        void IOnGameActions.OnStartCount() { }

        void IOnGameActions.OnEndCount() => StartGame();

        void IOnGameActions.OnRestartGame()
        {
            if (m_GameController.IsGameOver) return;

            m_TextForRecordTime.text = "Record: " + mRecordTime.ToString();
            NewGame();
        }

        void IOnGameActions.OnNextGame() { }

        void IOnGameActions.OnCloseGame() => StartTransitionAnim(ActivitesID.Instance.GetId<OptionsActivity>());

        public void OnResumeGame()
        {
            if (m_GameController.IsGameOver) return;

            m_SecundamerView.StartTime();
            m_Player.PlayGame();
        }

        private bool GetSavedData()
        {
            if (!GameDataLoader.LoadData(savedDataFileName, out SerializationGame data)) return false;
            this.SetSavedValue(data);
            GameDataLoader.DeleteData(savedDataFileName);
            GameDataLoader.DeleteData(MainClass.LastLoadedSceneId);
            return true;
        }

        private void SaveGameData()
        {
            var data = this.GetSavedValue();
            GameDataLoader.SaveData(data, savedDataFileName);
            GameDataLoader.SaveData(SceneId, MainClass.LastLoadedSceneId);
        }

        private void OnApplicationPause(bool pause)
        {
            if(m_GameController.IsCountStart || m_GameController.IsGameOver) return;

            else if (pause) SaveGameData();
            else GetSavedData();

            PauseGame();
        }

#if UNITY_EDITOR
        private void OnApplicationQuit()
        {
            if (m_GameController.IsCountStart || m_GameController.IsGameOver) return;
            SaveGameData();
        }
#endif

        #region Activites actions
        public override void StartActivity() => Screen.orientation = ScreenOrientation.Portrait;

        public override void OnBackPressed()
        {
            if (m_GameController.IsCountStart || m_GameController.IsGameOver) return;
            PauseGame();
        }

        public override void WaitActivity() => Finish();
        #endregion

        [Serializable] internal struct SerializationGame
        {
            [SerializedMember("GameType")] public int GameType;
            [SerializedMember("NumberOfArrays")] public int NumberOfArrays;
            [SerializedMember("TotalMillSec")] public double TotalMillSec;
            [SerializedMember("m_Player")] public SerializationPlayer Player;

            public static bool operator ==(SerializationGame game1, SerializationGame game2) => game1.Equals(game2);
            public static bool operator !=(SerializationGame game1, SerializationGame game2) => !(game1 == game2);
            public static bool operator !(SerializationGame game) => game != default;
            public static bool operator true(SerializationGame game) => game == default;
            public static bool operator false(SerializationGame game) => game != default;

            public override bool Equals(object obj)
            {
                if (obj == null || !(obj is SerializationGame game)) return false;
                else if (base.Equals(obj)) return true;

                return
                    game.GameType == GameType &&
                    game.NumberOfArrays == NumberOfArrays &&
                    game.TotalMillSec == TotalMillSec &&
                    game.Player.Equals(Player);
            }

            public override int GetHashCode()
            {
                int hashCode = -1776003746;
                hashCode = hashCode * -1521134295 + GameType.GetHashCode();
                hashCode = hashCode * -1521134295 + NumberOfArrays.GetHashCode();
                hashCode = hashCode * -1521134295 + TotalMillSec.GetHashCode();
                hashCode = hashCode * -1521134295 + Player.GetHashCode();
                return hashCode;
            }
        }
    }
}
