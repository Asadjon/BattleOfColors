using Assets.Scripts.PuzzleSolvers;
using Assets.Scripts.PuzzleSolvers.SolverClasses;
using Assets.Scripts.Resource;
using Assets.Scripts.SaveGameDatas.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Players
{
    [Serialization(typeof(SerializationAI_GameBoard))]
    sealed class AI_GameBoard : GameBoard, IAdapter
    {
        [SerializeField] private (float min, float max) m_LevelSwipeTime;

        private sbyte[] mFifteenGoalState;
        private PuzzleSolver mSolver;
        private List<Path> mSolution;
        private bool mIsRun;
        private sbyte[] mShuffledList;
        private GameSettings mSettings;

        private float mItemSwipeTime = 0;
        private float mCurrentTime = 0;

        private int SolverType
        {
            get => SerializationAI_GameBoard.GetEnumType(mSolver);
            set
            {
                StopAllCoroutines();
                (mSolver = PuzzleSolver.NewInstate(SerializationAI_GameBoard.GetPuzzleType(value)))
                    .Initialize(this, this);
            }
        }

        private int Level
        {
            get => (int)GameOptions.Instance.Level;
            set
            {
                GameOptions.Instance.Level = (GameOptions.GameLevels)value;
                m_LevelSwipeTime = GameOptions.Instance.GetLevelValue;
            }
        }

        private SerializePath[] Solution
        {
            get => mSolution.ConvertAll(p => (SerializePath)p).ToArray();
            set
            {
                mSolution = value.ToList().ConvertAll(p => (Path)p);
                mSolver.Next();
            }
        }

        protected override void LoadData()
        {
            base.LoadData();
            mSolution = new List<Path>();
            mSettings = GameSettings.Instance;
            m_LevelSwipeTime = GameOptions.Instance.GetLevelValue;
            mItemSwipeTime = GetSwipeTime();
        }

        protected override void CreateViews(List<ViewResource> resources)
        {
            base.CreateViews(resources);
            mFifteenGoalState = GetPuzzle();
            RestartGame();
        }

        void IAdapter.FoundSolution(List<Path> solution)
        {
            mSolution.AddRange(solution);
            if (mSolution.Count > 0) Debug.Log(mSolution[mSolution.Count - 1].PathToString());

            mSolver.Next();
        }

        private void Update()
        {
            if (mIsRun && !CheckTheWin())
            {
                mCurrentTime += Time.deltaTime;
                if (Run()) mCurrentTime = 0;
            }
            else if (mIsRun && CheckTheWin()) GameOver();
        }
        private bool Run()
        {
            if (mCurrentTime >= mItemSwipeTime && GetPath(out Path step))
            {
                var distanceOfSwiping = mSettings.DistanceOfSwiping;
                for (var i = 1; i < distanceOfSwiping; i++)
                    if (mSolution.Count == 0 || step.direction != mSolution[0].direction || !GetPath(out step)) break;

                SwitchSpecifiedPositions(step.moveTo - mEmptyNode.PositionInTheArray);

                mItemSwipeTime = GetSwipeTime();
                return true;
            }
            return false;
        }

        private bool GetPath(out Path path)
        {
            if (mSolution.Count > 0)
            {
                path = mSolution[0];
                mSolution.RemoveAt(0);
                return true;
            }

            path = default;
            return false;
        }

        private float GetSwipeTime() =>
            UnityEngine.Random.Range(m_LevelSwipeTime.min, m_LevelSwipeTime.max);

        public override void PauseGame() => mIsRun = false;

        public override void PlayGame() => mIsRun = true;

        public override void StartGame()
        {
            StartShuffle(mShuffledList);
            PlayGame();
        }

        public override void Restart()
        {
            base.Restart();
            RestartGame();
        }

        private void RestartGame()
        {
            mSolution.Clear();
            mShuffledList = mFifteenGoalState.Shuffle((PuzzleShuffle.ShuffleLevels)Level, mNumberOfArrays - GameOptions.MinNumberOfArrays);
            (mSolver = PuzzleSolver.NewInstate(GameType == GameOptions.GameTypes.WithNumber ? typeof(N_PuzzleSolver) : typeof(Color_PuzzleSolver)))
                .Initialize(this, this, mShuffledList.ToArray(), GetPuzzle()).Next();
        }

        protected override bool CheckTheWin() => !mSolver.WillItBeContinued && mSolution.Count <= 0;

        protected override void OnDestroy() => StopAllCoroutines();
    }

    [Serializable]
    sealed class SerializationAI_GameBoard : SerializationGameBoard
    {
        [SerializedMember("SolverType")] public int SolverType;
        [SerializedMember("mSolver")] public SerializeblePuzzleSolver Solver;
        [SerializedMember("Solution")] public SerializePath[] Solution;
        [SerializedMember("Level")] public int Level;

        public static int GetEnumType<T>(T solver) where T : PuzzleSolver
        {
            if (solver is N_PuzzleSolver) return 0;
            else if (solver is Color_PuzzleSolver) return 1;
            else throw new TypeAccessException();
        }

        public static Type GetPuzzleType(int solverType)
        {
            if (solverType == 0) return typeof(N_PuzzleSolver);
            else if (solverType == 1) return typeof(Color_PuzzleSolver);
            else throw new TypeAccessException();
        }
    }

    [Serializable]
    struct SerializePath
    {
        public sbyte[] puzzle;

        public int step;

        public SerializebleVector2Int moveTo;

        public byte direction;

        public static implicit operator SerializePath(Path path) => new SerializePath
        {
            puzzle = path.puzzle,
            step = path.step,
            moveTo = (SerializebleVector2Int) path.moveTo,
            direction = (byte)path.direction
        };

        public static explicit operator Path(SerializePath path) => new Path
        {
            puzzle = path.puzzle,
            step = path.step,
            moveTo = (Vector2Int) path.moveTo,
            direction = (Direction) path.direction
        };
    }
}