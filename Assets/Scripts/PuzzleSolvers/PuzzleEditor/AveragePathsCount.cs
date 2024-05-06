
using Assets.Scripts.Singletones;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Assets.Scripts.GameOptions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets.Scripts.PuzzleSolvers.PuzzleEditor
{
    public class AveragePathsCount : SingletoneForScriptableObject<AveragePathsCount>
    {
        [SerializeField] private Types AI_MovesCount = new Types();

        internal void Add(GameTypes gameType, GameLevels gameLevel, SizesOfSquare sizesOfSquare, int movesCount)
        {
            var movesCountList = AI_MovesCount[gameType][sizesOfSquare][gameLevel];

            if (movesCountList.Contains(movesCount)) return;

            movesCountList.Add(movesCount);

            movesCountList.Sort();
            movesCountList.Reverse();

            if (movesCountList.Count > 10)
                movesCountList.RemoveRange(10, movesCountList.Count - 10);

            if (movesCountList.Count < movesCountList.Capacity) movesCountList.Capacity = movesCountList.Count;
        }

        internal int Lerp(GameTypes gameType, GameLevels gameLevel, SizesOfSquare sizesOfSquare, float t)
        {
            var movesCount = AI_MovesCount[gameType][sizesOfSquare][gameLevel];

            if (movesCount.Count <= 0) return 100;

            t = Mathf.Clamp(t, 0f, 1f);
            var lerpIndex = (int)Mathf.Lerp(0, movesCount.Count - 1, t);
            return movesCount[lerpIndex];
        }

        internal int Max(GameTypes gameType, GameLevels gameLevel, SizesOfSquare sizesOfSquare)
        {
            var movesCount = AI_MovesCount[gameType][sizesOfSquare][gameLevel];
            return movesCount.Count > 0 ? movesCount.Max() : 100;
        }

        internal int Min(GameTypes gameType, GameLevels gameLevel, SizesOfSquare sizesOfSquare)
        {
            var movesCount = AI_MovesCount[gameType][sizesOfSquare][gameLevel];
            return movesCount.Count > 0 ? movesCount.Min() : 100;
        }

#if UNITY_EDITOR
        private void Reset() => AI_MovesCount = new Types();

        [MenuItem("Tools/Singletons/Paths Count")]
#endif
        [RuntimeInitializeOnLoadMethod]
        static void Create() => Create("Assets/Resources/AI Paths Count.asset");
    }

    [Serializable]
    internal class Levels
    {
        [SerializeField] private List<Level> m_Levels = new List<Level>();

        public List<int> this[GameLevels level] => m_Levels.Find(l => l.level == level).movesCount;

        public Levels() : base()
        {
            foreach (GameLevels level in typeof(GameLevels).GetEnumValues())
                m_Levels.Add(new Level { level = level, movesCount = new List<int>() });
        }

        [Serializable] internal struct Level { public GameLevels level; public List<int> movesCount; }
    }

    [Serializable]
    internal class Sizes
    {
        [SerializeField] private List<Size> m_Sizes = new List<Size>();

        public Levels this[SizesOfSquare size] => m_Sizes.Find(s => s.sizesOfSquare == size).levels;

        public Sizes() : base()
        {
            foreach (SizesOfSquare size in typeof(SizesOfSquare).GetEnumValues())
                m_Sizes.Add(new Size { sizesOfSquare = size, levels = new Levels() });
        }

        [Serializable] internal struct Size { public SizesOfSquare sizesOfSquare; public Levels levels; }
    }

    [Serializable]
    internal class Types
    {
        [SerializeField] private List<Type> m_Types = new List<Type>();

        public Sizes this[GameTypes type] => m_Types.Find(t => t.gameTypes == type).sizes;

        public Types() : base()
        {
            foreach (GameTypes type in typeof(GameTypes).GetEnumValues())
                m_Types.Add(new Type { gameTypes = type, sizes = new Sizes() });
        }

        [Serializable] internal struct Type { public GameTypes gameTypes; public Sizes sizes; }
    }
}