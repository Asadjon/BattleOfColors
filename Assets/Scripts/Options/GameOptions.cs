using Assets.Scripts.Records;
using Assets.Scripts.Singletones;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets.Scripts
{
    internal sealed class GameOptions : SingletoneForScriptableObject<GameOptions>
    {
        public enum GameLevels { Easy = 0, Normal = 1, Hard = 2, Expert = 3 }

        public enum GameTypes { WithColor = 0, WithNumber = 1 }

        public enum SizesOfSquare { _3x3 = 3, _4x4 = 4, _5x5 = 5, _6x6 = 6, _7x7 = 7, _8x8 = 8 }

        #region Constantas
        public const SizesOfSquare DefaultSizeOfSquare = SizesOfSquare._3x3;
        public const SizesOfSquare MinSizeOfSquare = SizesOfSquare._3x3;
        public const SizesOfSquare MaxSizeOfSquare = SizesOfSquare._8x8;
        public const GameTypes DefaultGameType = GameTypes.WithColor;
        public const GameLevels DefaultGameLevel = GameLevels.Normal;
        #endregion

        #region SerializeField Objects
        [SerializeField] private SizesOfSquare m_SizeOfSquare = DefaultSizeOfSquare;
        [SerializeField] private GameLevels m_GameLevel = DefaultGameLevel;
        [SerializeField] private GameTypes m_GameType = DefaultGameType;
        #endregion

        #region Getters And Setters
        public SizesOfSquare SizeOfSquar { get => m_SizeOfSquare; set { m_SizeOfSquare = value; GetRecordData(); } }
        public GameTypes GameType { get => m_GameType; set { m_GameType = value; GetRecordData(); } }
        public GameLevels GameLevel { get => m_GameLevel; set { m_GameLevel = value; GetRecordData(); } }
        public RecordData RecordData => mRecordData;
        public sbyte[] GoalState
        {
            get
            {
                var goal = new List<int>(Enumerable.Range(1, (int)Math.Pow(m_SizeOfSquare.Value(), 2) - 1)) { 0 };
                return goal.ConvertAll(i => (sbyte)i).ToArray();
            }
        }
        #endregion

        private RecordData mRecordData = null;
        private void GetRecordData() => mRecordData = RecordController.Instance[m_GameType][m_SizeOfSquare][m_GameLevel];

        private void LoadData()
        {
            m_SizeOfSquare = DefaultSizeOfSquare;
            m_GameLevel = DefaultGameLevel;
            m_GameType = DefaultGameType;

            GetRecordData();
        }

        [RuntimeInitializeOnLoadMethod]
        static void Load() => Instance?.LoadData();


        [RuntimeInitializeOnLoadMethod]
#if UNITY_EDITOR
        [MenuItem("Tools/Singletons/Game Options")]
#endif
        static void Create() => Create("Assets/Resources/Game Options.asset");
    }
    static class SizesOfSquareHelper
    {
        public static int Value(this GameOptions.SizesOfSquare size) => (int)size;
    }
}
