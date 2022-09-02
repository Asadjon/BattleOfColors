using System;
using UnityEngine;
using Assets.Scripts.Players;
using Assets.Scripts.Resource;
using Assets.Scripts.UI;
using Assets.Scripts.GameControllers;
using Assets.Scripts.SaveGameDatas;

using static Assets.Scripts.GameControllers.GameController;
using static Assets.Scripts.Activitys.MultiplayerGameActivity;

namespace Assets.Scripts.Activitys
{
    class MultiplayerGameActivity : Activity, IOnGameActions, ISerialized<SerializationGame>
    {
        [SerializeField] private Player m_UserPlayer = null;
        [SerializeField] private Player m_AIPlayer = null;
        [SerializeField] private GameController m_GameController = null;
        [SerializeField] private SecundamerView m_SecundamerView = null;
        [SerializeField] private Gradient m_SecundamerGradient = null;
        [SerializeField] private string m_PushSoundName = default;

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
            var gameOptions = GameOptions.Instance;
            var game = GetSavedData();

            if (!game)
            {
                OptionsActivity.ActivityType = SceneId;
                gameOptions.GameType = game.GameType;
                gameOptions.NumberOfArrays = game.NumberOfArrays;
            }

            var resurces = ViewResource.GenerateResources(gameOptions.NumberOfArrays);

            m_UserPlayer.GameType = gameOptions.GameType;
            m_UserPlayer.OnGameOver.AddListener(GameOver);
            m_UserPlayer.AddSwipeAction(() => PlaySound(m_PushSoundName));
            m_UserPlayer.InitializeGame(gameOptions.NumberOfArrays, resurces);

            m_AIPlayer.GameType = gameOptions.GameType;
            m_AIPlayer.OnGameOver.AddListener(GameOver);
            m_AIPlayer.AddSwipeAction(() => PlaySound(m_PushSoundName));
            m_AIPlayer.InitializeGame(gameOptions.NumberOfArrays, resurces);

            mRecordTime = RecordHelper.GetRecord(gameOptions.NumberOfArrays, gameOptions.GameType).time;

            m_SecundamerView.ResetTime();
            m_SecundamerView.TextFormat = "hh\\\n\\·\\·\\\nmm\\\n\\·\\·\\\nss";
            m_SecundamerView.OnValueChange.AddListener(time =>
                m_SecundamerView.ChangeTextColor(
                    m_SecundamerGradient.Evaluate(
                        Mathf.Clamp(mRecordTime.TotalMilliseconds != 0 ? (float)(time.TotalMilliseconds / mRecordTime.TotalMilliseconds) : 0, 0, 1))));

            m_GameController.SetGameActions(this);

            StartGame(game);
        }

        public void PlaySound(string soundName) => AudioManager.Instance.Play(soundName);

        private void StartGame(SerializationGame game)
        {
            if (!game)
            {
                m_UserPlayer.Set(game.UserPlayer);
                m_AIPlayer.Set(game.UserPlayer);
                m_SecundamerView.Value = game.TotalMillSec;
                PauseGame();
            }
            else m_GameController.StartGame();
        }

        private void StartGame()
        {
            m_SecundamerView.StartTime();
            m_UserPlayer.StartGame();
            m_AIPlayer.StartGame();
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

        private SerializationGame GetSavedData()
        {
            var data = GameDataLoader.LoadData<SerializationGame>(savedDataFileName);
            GameDataLoader.DeleteData(savedDataFileName);
            GameDataLoader.DeleteData(MainClass.LastLoadedSceneId);

            return data;
        }

        private void SaveGameData()
        {
            GameDataLoader.SaveData(Serialize(default), savedDataFileName);
            GameDataLoader.SaveData(SceneId, MainClass.LastLoadedSceneId);
        }

        public SerializationGame Serialize(SerializationGame game) => new SerializationGame
        {
            GameType = GameOptions.Instance.GameType,
            NumberOfArrays = GameOptions.Instance.NumberOfArrays,
            UserPlayer = m_UserPlayer,
            AIPlayer = m_AIPlayer,
            TotalMillSec = m_SecundamerView.Value
        };

        public void Deserialize(SerializationGame game)
        {
            OptionsActivity.ActivityType = SceneId;
            GameOptions.Instance.GameType = game.GameType;
            GameOptions.Instance.NumberOfArrays = game.NumberOfArrays;
            m_UserPlayer.Set(game.UserPlayer);
            m_AIPlayer.Set(game.AIPlayer);
            m_SecundamerView.Value = game.TotalMillSec;
        }

        private void OnApplicationPause(bool pause)
        {
            if(m_GameController.IsCountStart || m_GameController.IsGameOver) return;

            else if (pause)
            {
                SaveGameData();
                PauseGame();
                return;
            }
            else if (GetSavedData() is SerializationGame game && !game)
                Deserialize(game);

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
            public GameOptions.GameTypes GameType;
            public int NumberOfArrays;
            public double TotalMillSec;
            public SerializationPlayer UserPlayer;
            public SerializationPlayer AIPlayer;

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
