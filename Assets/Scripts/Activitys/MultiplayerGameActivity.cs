using System;
using UnityEngine;
using Assets.Scripts.Players;
using Assets.Scripts.Resource;
using Assets.Scripts.UI;
using Assets.Scripts.GameControllers;
using Assets.Scripts.SaveGameDatas;

using static Assets.Scripts.GameControllers.GameController;
using Assets.Scripts.SaveGameDatas.Attributes;
using static Assets.Scripts.GameOptions;
using Assets.Scripts.Records;
using System.Collections.Generic;
using Assets.Scripts.PuzzleSolvers;
using Assets.Scripts.AudioManagers;

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

        private List<ViewResource> mResources = null;
        protected sbyte[] mGoalState;
        //protected sbyte[] mShuffledList;

        private RecordData mRecordData => GameOptions.Instance.RecordData;

        private const string savedDataFileName = "MultiGame.dat";

        public override void OnCreate(Bundle bundle)
        {
        }

        protected override void Start()
        {
            base.Start(); 
            LoadData();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            m_UserPlayer.CalculateSize((int)GameOptions.Instance.SizeOfSquar);
            m_AIPlayer.CalculateSize((int)GameOptions.Instance.SizeOfSquar);
        }

        private void LoadData()
        {
            m_UserPlayer.OnGameOver.AddListener(GameOver);
            m_UserPlayer.AddSwipeAction(_ => PlaySound(m_PushSoundName));

            m_AIPlayer.OnGameOver.AddListener(GameOver);
            //m_AIPlayer.AddSwipeAction(_ => PlaySound(m_PushSoundName));

            m_GameController.SetGameActions(this);

            m_SecundamerView.TextFormat = /*hh\\\n\\·\\·\\\n*/"mm\\\n\\·\\·\\\nss";
            m_SecundamerView.Inverse = true;

            var isSaveDataReset = GetSavedData();
            var gameOptions = GameOptions.Instance;

            mGoalState = gameOptions.GoalState;
            sbyte[] shuffleState = null;

            if (isSaveDataReset) PauseGame();
            else
            {
                mResources = ((int)gameOptions.SizeOfSquar).GenerateResources();
                m_SecundamerView.MaxTime = TimeSpan.FromSeconds((double)mRecordData.GetAverage(RecordData.Parametrs.Time));
                m_SecundamerView.ResetTime();
                m_GameController.StartGame();

                shuffleState = Shuffle();
            }

            m_UserPlayer.InitializeGame(mGoalState, gameOptions.SizeOfSquar, gameOptions.GameType, gameOptions.GameLevel);
            m_AIPlayer.InitializeGame(mGoalState, gameOptions.SizeOfSquar, gameOptions.GameType, gameOptions.GameLevel);

            m_UserPlayer.NewGame(mResources, shuffleState);
            m_AIPlayer.NewGame(mResources, shuffleState);

            m_SecundamerView.OnValueChange.AddListener(time =>
                m_SecundamerView.TextColor = m_SecundamerGradient.Evaluate(1f - Mathf.Clamp(mRecordData.Sum != 0 ? (float)(time.TotalMilliseconds / mRecordData.RecordTime.TotalMilliseconds) : 0, 0, 1)));
            m_SecundamerView.OnValueLimited.AddListener(_ => GameOver(null));
        }

        public void PlaySound(string soundName) => AudioManager.Instance.Play(soundName);

        private void StartGame()
        {
            m_SecundamerView.StartTime();
            m_UserPlayer.StartGame();
            m_AIPlayer.StartGame();
        }

        private void GameOver(Player player)
        {
            if (m_GameController.IsGameOver) return;

            m_SecundamerView.StopTime();

            var message = "<color=white>Game over!</color>";

            if (player == m_UserPlayer)
                message = "<color=green>You won!</color>";
            else if (player == m_AIPlayer)
                message = "<color=red>You didn‘t win!</color>";

            m_GameController.SetMessage(message);
            m_GameController.GameOver();
            m_UserPlayer.StopGame();
            m_AIPlayer.StopGame();
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
            m_SecundamerView.MaxTime = TimeSpan.FromSeconds((double)mRecordData.GetAverage(RecordData.Parametrs.Time));
            m_SecundamerView.ResetTime();

            var shuffle = Shuffle();
            m_UserPlayer.NewGame(mResources, shuffle);
            m_AIPlayer.NewGame(mResources, shuffle);
        }

        public sbyte[] Shuffle()
        { 
            mResources = mResources?.Shuffle();
            return mGoalState.Shuffle(GameOptions.Instance.GameLevel,
                Array.FindIndex((SizesOfSquare[])typeof(SizesOfSquare).GetEnumValues(), size => size == GameOptions.Instance.SizeOfSquar));
        }

        void IOnGameActions.OnStartCount() { }

        void IOnGameActions.OnEndCount() => StartGame();

        void IOnGameActions.OnRestartGame()
        {
            if (m_GameController.IsGameOver) return;

            NewGame();
        }

        void IOnGameActions.OnNextGame() { }

        void IOnGameActions.OnCloseGame() => Finish();

        public void OnResumeGame()
        {
            if (m_GameController.IsGameOver) return;

            m_SecundamerView.StartTime();
            m_UserPlayer.PlayGame();
            m_AIPlayer.PlayGame();
        }

        public static bool TryGetSavedData(out SerializationGame data) =>
            GameDataLoader.LoadData(savedDataFileName, out data);

        private bool GetSavedData()
        {
            if (!TryGetSavedData(out SerializationGame data)) return false;
            try
            { this.SetSavedValue(data); }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
            GameDataLoader.DeleteData(savedDataFileName);

            return true;
        }

        private void SaveGameData()
        {
            if (GameDataLoader.LoadData<SerializationGame>(savedDataFileName, out _)) return;
            GameDataLoader.SaveData(this.GetSavedValue(), savedDataFileName);
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
        public override void OnPlay() => Screen.orientation = ScreenOrientation.LandscapeLeft;

        public override void OnBackPressed()
        {
            if (m_GameController.IsCountStart || m_GameController.IsGameOver) return;
            PauseGame();
        }
        #endregion

        #region Serialize game datas

        private GameLevels GameLevel { get => GameOptions.Instance.GameLevel; set => GameOptions.Instance.GameLevel = value; }
        private GameTypes GameType { get => GameOptions.Instance.GameType; set => GameOptions.Instance.GameType = value; }
        private SizesOfSquare SizeOfSquare { get => GameOptions.Instance.SizeOfSquar; set => GameOptions.Instance.SizeOfSquar = value; }
        private TimeSpan CurrentTime { get => m_SecundamerView.CurrentTime; set => m_SecundamerView.CurrentTime = value; }
        private DisplayedTimeSpan MaxTime { get => m_SecundamerView.MaxTime; set => m_SecundamerView.MaxTime = (TimeSpan)value; }

        [Serializable]
        internal struct SerializationGame
        {
            [SerializedMember("GameType")] public GameTypes GameType;
            [SerializedMember("GameLevel")] public GameLevels GameLevel;
            [SerializedMember("SizeOfSquare")] public SizesOfSquare SizeOfSquare;
            [SerializedMember("CurrentTime")] public TimeSpan CurrentTime;
            [SerializedMember("MaxTime")] public DisplayedTimeSpan MaxTime;
            [SerializedMember("mResources")] public List<MyViewResource> Resources;
            [SerializedMember("m_AIPlayer")] public SerializationPlayer AIPlayer;
            [SerializedMember("m_UserPlayer")] public SerializationPlayer UserPlayer;
        }
        #endregion
    }
}
