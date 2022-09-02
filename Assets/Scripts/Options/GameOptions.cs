using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts
{
    internal class GameOptions : Singltone<GameOptions>, ISerializationCallbackReceiver
    {
        public enum GameLevels { Easy = 0, Normal = 1, Hard = 2, Expert = 3 }
        public enum GameTypes { WithColor = 0, WithNumber = 1 }

        #region Constantas
        private static readonly (float, float)[] Levels = new (float, float)[] { (1f, 1.5f), (.5f, .75f), (.25f, .375f), (.125f, .1875f) };
        public const int DefaultNumberOfArrays = 3;
        public const int MinNumberOfArrays = 3;
        public const int MaxNumberOfArrays = 8;
        public const GameTypes DefaultGameType = GameTypes.WithColor;
        public const GameLevels DefaultGameLevel = GameLevels.Normal;
        #endregion

        #region SerializeField Objects
        [SerializeField, Range(MinNumberOfArrays, MaxNumberOfArrays)] private int m_NumberOfArrays = DefaultNumberOfArrays;
        [SerializeField] private GameLevels m_Level = DefaultGameLevel;
        [SerializeField] private GameTypes m_GameType = DefaultGameType;
        [SerializeField] private UnityEvent<int> m_OnChangeNumberOfArrays;
        #endregion

        #region Getters And Setters
        public int NumberOfArrays { get => m_NumberOfArrays; set => m_NumberOfArrays = Mathf.Clamp(value, MinNumberOfArrays, MaxNumberOfArrays); }
        public GameTypes GameType { get => m_GameType; set => m_GameType = value; }
        public UnityEvent<int> OnChangeNumberOfArrays => m_OnChangeNumberOfArrays;

        private (float, float) mLevelValue;
        public (float min, float max) GetLevelValue => mLevelValue;
        public GameLevels Level { get => m_Level; set
            {
                m_Level = value;
                mLevelValue = Levels[(int)m_Level];
            }
        }
        #endregion

        protected override void LoadData() =>
            Level = m_Level;

        private int mOldNumberOfArrays;
        public void OnBeforeSerialize()
        {
            if (mOldNumberOfArrays != m_NumberOfArrays)
            {
                mOldNumberOfArrays = m_NumberOfArrays;
                m_OnChangeNumberOfArrays.Invoke(m_NumberOfArrays);
            }
        }

        public void OnAfterDeserialize()
        {

        }
    }
}
