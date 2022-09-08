using System;
using UnityEngine;
using Assets.Scripts.Players;
using Assets.Scripts.Resource;
using Assets.Scripts.UI;
using Assets.Scripts.GameControllers;
using Assets.Scripts.SaveGameDatas;

using static Assets.Scripts.GameControllers.GameController;
using static Assets.Scripts.Activitys.MultiplayerGameActivity;
using Assets.Scripts.SaveGameDatas.Attributes;

namespace Assets.Scripts.Activitys
{
    [Serialization(typeof(SerializationGame))]
    class MultiplayerGameActivity : Activity, IOnGameActions
    {
        [SerializeField] private Player m_UserPlayer = null;
        [SerializeField] private Player m_AIPlayer = null;
        [SerializeField] private GameController m_GameController = null;
        [SerializeField] private SecundamerView m_SecundamerView = null;
        [SerializeField] private Gradient m_SecundamerGradient = null;
        [SerializeField] private string m_PushSoundName = default;

        private int GameType { get => (int)GameOptions.Instance.GameType; set
            {
                GameOptions.Instance.GameType = (GameOptions.GameTypes)value;
                m_UserPlayer.GameType = GameOptions.Instance.GameType;
                m_AIPlayer.GameType = GameOptions.Instance.GameType;
            }
        }
        private int NumberOfArrays
        {
            get => GameOptions.Instance.NumberOfArrays;
            set
            {
                GameOptions.Instance.NumberOfArrays = value;
                mRecordTime = RecordHelper.GetRecord(value, GameOptions.Instance.GameType).time;
                m_UserPlayer.InitializeGame(value, ViewResource.GenerateResources(value));
                m_AIPlayer.InitializeGame(value, ViewResource.GenerateResources(value));
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

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            m_UserPlayer.CalculateSize(GameOptions.Instance.NumberOfArrays);
            m_AIPlayer.CalculateSize(GameOptions.Instance.NumberOfArrays);
        }

        private void LoadData()
        {
            OptionsActivity.ActivityType = SceneId;
            var gameOptions = GameOptions.Instance;

            m_UserPlayer.PlayerName = "User player";
            m_UserPlayer.OnGameOver.AddListener(GameOver);
            m_UserPlayer.AddSwipeAction(() => PlaySound(m_PushSoundName));

            m_AIPlayer.PlayerName = "AI player";
            m_AIPlayer.OnGameOver.AddListener(GameOver);
            m_AIPlayer.AddSwipeAction(() => PlaySound(m_PushSoundName));

            mRecordTime = RecordHelper.GetRecord(gameOptions.NumberOfArrays, gameOptions.GameType).time;

            m_SecundamerView.TextFormat = "hh\\\n\\·\\·\\\nmm\\\n\\·\\·\\\nss";
            m_SecundamerView.OnValueChange.AddListener(time =>
                m_SecundamerView.ChangeTextColor(
                    m_SecundamerGradient.Evaluate(
                        Mathf.Clamp(mRecordTime.TotalMilliseconds != 0 ? (float)(time.TotalMilliseconds / mRecordTime.TotalMilliseconds) : 0, 0, 1))));

            m_GameController.SetGameActions(this);

            if (GetSavedData()) PauseGame();
            else
            {
                var resurces = ViewResource.GenerateResources(gameOptions.NumberOfArrays);

                m_UserPlayer.GameType = gameOptions.GameType;
                m_UserPlayer.InitializeGame(gameOptions.NumberOfArrays, resurces);

                m_AIPlayer.GameType = gameOptions.GameType;
                m_AIPlayer.InitializeGame(gameOptions.NumberOfArrays, resurces);

                m_SecundamerView.ResetTime();

                m_GameController.StartGame();
            }
        }

        public void PlaySound(string soundName) => AudioManager.Instance.Play(soundName);

        private void StartGame()
        {
            m_SecundamerView.StartTime();
            m_UserPlayer.StartGame();
            m_AIPlayer.StartGame();
        }

        private void GameOver(string playerName, int _)
        {
            m_SecundamerView.StopTime();

            var message = "You won!";
            var messageColor = Color.green;

            if (playerName == m_AIPlayer.PlayerName)
            {
                message = "You didn‘t win!";
                messageColor = Color.red;
            }

            m_GameController.SetMessage(message, messageColor);
            m_GameController.GameOver();
            m_UserPlayer.PauseGame();
            m_AIPlayer.PauseGame();
        }

        public void PauseGameForSettings()
        {
            m_SecundamerView.StopTime();
            m_UserPlayer.PauseGame();
            m_AIPlayer.PauseGame();
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
            m_UserPlayer.NewGame();
            m_AIPlayer.NewGame();
        }

        void IOnGameActions.OnStartCount() { }

        void IOnGameActions.OnEndCount() => StartGame();

        void IOnGameActions.OnRestartGame()
        {
            if (m_GameController.IsGameOver) return;

            NewGame();
        }

        void IOnGameActions.OnNextGame() { }

        void IOnGameActions.OnCloseGame() => StartTransitionAnim(ActivitesID.Instance.GetId<OptionsActivity>());

        public void OnResumeGame()
        {
            if (m_GameController.IsGameOver) return;

            m_SecundamerView.StartTime();
            m_UserPlayer.PlayGame();
            m_AIPlayer.PlayGame();
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
        public override void StartActivity() => Screen.orientation = ScreenOrientation.Landscape;

        public override void OnBackPressed()
        {
            if (m_GameController.IsCountStart || m_GameController.IsGameOver) return;
            PauseGame();
        }

        public override void WaitActivity() => Finish();
        #endregion

        [Serializable]
        internal struct SerializationGame
        {
            [SerializedMember("GameType")] public int GameType;
            [SerializedMember("NumberOfArrays")] public int NumberOfArrays;
            [SerializedMember("TotalMillSec")] public double TotalMillSec;
            [SerializedMember("m_AIPlayer")] public SerializationPlayer AIPlayer;
            [SerializedMember("m_UserPlayer")] public SerializationPlayer UserPlayer;

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
                    game.UserPlayer.Equals(UserPlayer) &&
                    game.AIPlayer.Equals(AIPlayer);
            }
        }
    }
}
