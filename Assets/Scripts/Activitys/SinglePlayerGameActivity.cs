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

namespace Assets.Scripts.Activitys
{
    class SinglePlayerGameActivity : Activity, IOnGameActions, ISerialized<SerializationGame>
    {
        [SerializeField] private Player m_Player = null;
        [SerializeField] private GameController m_GameController = null;
        [SerializeField] private SecundamerView m_SecundamerView = null;
        [SerializeField] private TextMeshProUGUI m_TextForRecordTime = null;
        [SerializeField] private Gradient m_SecundamerGradient = null;
        [SerializeField] private string m_PushSoundName = default;

        private TimeSpan mRecordTime;

        private const string savedDataFileName = "PlayGame.dat";

        protected override void Start()
        {
            base.Start();
            LoadData();
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

            m_Player.GameType = gameOptions.GameType;
            m_Player.OnGameOver.AddListener(GameOver);
            m_Player.AddSwipeAction(() => PlaySound(m_PushSoundName));
            m_Player.InitializeGame(GameOptions.Instance.NumberOfArrays, 
                ViewResource.GenerateResources(GameOptions.Instance.NumberOfArrays, res => res.Set(res.Id, string.Empty, res.Color)));

            mRecordTime = RecordHelper.GetRecord(GameOptions.Instance.NumberOfArrays, gameOptions.GameType).time;
            m_TextForRecordTime.text = "Record: " + mRecordTime.ToString(@"hh\:mm\:ss");

            m_SecundamerView.ResetTime();
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
                m_Player.Set(game.Player);
                m_SecundamerView.Value = game.TotalMillSec;
                PauseGame();
            }
            else m_GameController.StartGame();
        }

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
            Player = m_Player,
            TotalMillSec = m_SecundamerView.Value
        };

        public void Deserialize(SerializationGame game)
        {
            OptionsActivity.ActivityType = SceneId;
            GameOptions.Instance.GameType = game.GameType;
            GameOptions.Instance.NumberOfArrays = game.NumberOfArrays;
            m_Player.Set(game.Player);
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
            {
                Deserialize(game);
            }

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
            public GameOptions.GameTypes GameType;
            public int NumberOfArrays;
            public double TotalMillSec;
            public SerializationPlayer Player;

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
        }
    }
}
