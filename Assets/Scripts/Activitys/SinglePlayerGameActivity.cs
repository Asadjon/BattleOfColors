using System;
using UnityEngine;
using TMPro;
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
    class SinglePlayerGameActivity : Activity, IOnGameActions
    {
        [SerializeField] private Player m_Player;
        [SerializeField] private GameController m_GameController = null;
        [SerializeField] private SecundamerView m_SecundamerView = null;
        [SerializeField] private TextMeshProUGUI m_TextForMovesCount = null;
        [SerializeField] private TextMeshProUGUI m_TextForRecord = null;
        [SerializeField] private Gradient m_RecordGradient = null;

        private List<ViewResource> mResources = null;
        protected sbyte[] mGoalState;

        private RecordData mRecordData => GameOptions.Instance.RecordData;
        private UnityEngine.Events.UnityAction<int> mOnChangeValues;

        private const string savedDataFileName = "SingleGame.dat";

        protected override void Start()
        {
            base.Start();
            LoadData();
        }

        private void LoadData()
        {
            mOnChangeValues = movesCount => {
                m_TextForMovesCount.text = movesCount.ToString();
                m_TextForMovesCount.color =
                    m_RecordGradient.Evaluate(
                        Mathf.Clamp(mRecordData.Sum != 0 ? (float)movesCount / mRecordData.MovesCount : 0, 0, 1));
            };

            m_SecundamerView.OnValueChange.AddListener(time =>
            {
                m_SecundamerView.TextColor =
                    m_RecordGradient.Evaluate(
                        Mathf.Clamp(mRecordData.Sum != 0 ? (float)(time.TotalMilliseconds / mRecordData.RecordTime.TotalMilliseconds) : 0, 0, 1));
            });

            m_SecundamerView.OnValueLimited.AddListener(time => GameOver(null));
            m_SecundamerView.MaxTime = TimeSpan.FromHours(1);

            m_GameController.SetGameActions(this);

            m_Player.OnGameOver.AddListener(GameOver);
            m_Player.AddSwipeAction(mOnChangeValues);

            var isSaveDataReset = GetSavedData();
            var gameOptions = GameOptions.Instance;

            mGoalState = gameOptions.GoalState;
            sbyte[] shuffleState = null;

            if (isSaveDataReset) PauseGame();
            else
            {
                mResources = gameOptions.SizeOfSquar.Value().GenerateResources();
                m_SecundamerView.ResetTime();
                m_GameController.StartGame();

                shuffleState = Shuffle();
            }

            m_Player.InitializeGame(mGoalState, gameOptions.SizeOfSquar, gameOptions.GameType, gameOptions.GameLevel);
            m_Player.NewGame(mResources, shuffleState);

            m_TextForRecord.text = mRecordData.RecordTime.ToString(@"mm\:ss") + " / " + mRecordData.MovesCount;
            mOnChangeValues.Invoke(m_Player.MovesCount);
        }

        public void PlaySound(string soundName) => AudioManager.Instance.Play(soundName);

        private void StartGame()
        {
            m_SecundamerView.StartTime();
            m_Player.StartGame();
        }

        private void GameOver(Player player)
        {
            m_SecundamerView.StopTime();

            var message = "<color=white>Time is up</color>";

            if (player)
            {
                message = "Time: " + m_SecundamerView.ToString() + "\nMoves: " + player.MovesCount;
                message = mRecordData.TrySetData(m_SecundamerView.CurrentTime, player.MovesCount) ?
                "<color=green><size=120>New record</size>\n<size=70>" + message + "</size></color>" : "<color=white>" + message + "</color>";
            }

            m_GameController.SetMessage(message);
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
            m_SecundamerView.ResetTime();

            var shuffle = Shuffle();
            m_Player.NewGame(mResources, shuffle);
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
            m_TextForRecord.text = mRecordData.RecordTime.ToString(@"hh\:mm\:ss") + " / " + mRecordData.MovesCount;
            mOnChangeValues.Invoke(m_Player.MovesCount);
        }

        void IOnGameActions.OnNextGame() { }

        void IOnGameActions.OnCloseGame() => Finish();

        public void OnResumeGame()
        {
            if (m_GameController.IsGameOver) return;

            m_SecundamerView.StartTime();
            m_Player.PlayGame();
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
        public override void OnPlay() => Screen.orientation = ScreenOrientation.Portrait;

        public override void OnBackPressed()
        {
            if (m_GameController.IsCountStart || m_GameController.IsGameOver) return;
            PauseGame();
        }

        public override void OnCreate(Bundle bundle)
        {
        }
        #endregion

        #region Serialize game datas

        private GameLevels GameLevel { get => GameOptions.Instance.GameLevel; set => GameOptions.Instance.GameLevel = value; }

        private GameTypes GameType { get => GameOptions.Instance.GameType; set => GameOptions.Instance.GameType = value; }
        private SizesOfSquare SizeOfSquare { get => GameOptions.Instance.SizeOfSquar; set => GameOptions.Instance.SizeOfSquar = value; }
        private DisplayedTimeSpan CurrentTime { get => m_SecundamerView.CurrentTime; set => m_SecundamerView.CurrentTime = (TimeSpan)value; }
        private DisplayedTimeSpan MaxTime { get => m_SecundamerView.MaxTime; set => m_SecundamerView.MaxTime = (TimeSpan)value; }

        [Serializable] internal struct SerializationGame
        {
            [SerializedMember("GameType")] public GameTypes GameType;
            [SerializedMember("GameLevel")] public GameLevels GameLevel;
            [SerializedMember("SizeOfSquare")] public SizesOfSquare SizeOfSquare;
            [SerializedMember("CurrentTime")] public DisplayedTimeSpan CurrentTime;
            [SerializedMember("MaxTime")] public DisplayedTimeSpan MaxTime;
            [SerializedMember("mResources")] public List<MyViewResource> Resources;
            [SerializedMember("m_Player")] public SerializationPlayer Player;
        }
        #endregion
    }
}
