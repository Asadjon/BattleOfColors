using Assets.Scripts.PuzzleSolvers;
using Assets.Scripts.PuzzleSolvers.SolverClasses;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Players
{
    sealed class AI_GameBoard : GameBoard, IAdapter
    {
        private sbyte[] mFifteenGoalState;
        private PuzzleSolver mSolver;
        private List<Path> mSolution;
        private bool mIsRun;
        private List<sbyte> mShuffledList;
        private GameSettings mSettings;
        private GameOptions mOptions;

        private float mItemSwipeTime = 0;
        private float mCurrentTime = 0;

        [SerializeField] private (float min, float max) m_LevelSwipeTime;

        protected override SerializationGameBoard CreateMyGameBoard() => new SerializationAI_GameBoard
        {
            SolverType = SerializationAI_GameBoard.GetEnumType(mSolver),
            Solver = mSolver.Implicit(),
            Solution = mSolution.ConvertAll(p => (SerializePath)p),
            Level = mOptions.Level
        };

        public override GameBoard Set(SerializationGameBoard myGameBoard)
        {
            base.Set(myGameBoard);

            if (!(myGameBoard is SerializationAI_GameBoard gameBoard)) return this;

            StopAllCoroutines();
            mSolution = gameBoard.Solution.ConvertAll(p => (Path)p);
            (mSolver = PuzzleSolver.NewInstate(SerializationAI_GameBoard.GetPuzzleType(gameBoard.SolverType)))
                .Initialize(this, this).Set(gameBoard.Solver).Next();

            mOptions.Level = gameBoard.Level;
            m_LevelSwipeTime = mOptions.GetLevelValue;
            mItemSwipeTime = GetSwipeTime();

            return this;
        }

        protected override void LoadData()
        {
            base.LoadData();
            mSolution = new List<Path>();
            mSettings = GameSettings.Instance;
            mOptions = GameOptions.Instance;
            m_LevelSwipeTime = GameOptions.Instance.GetLevelValue;
            mItemSwipeTime = GetSwipeTime();
        }

        protected override void CreateViews()
        {
            base.CreateViews();
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
            mShuffledList = mFifteenGoalState.ShuffleIsSolvable(GameType == GameOptions.GameTypes.WithNumber);
            (mSolver = PuzzleSolver.NewInstate(GameType == GameOptions.GameTypes.WithNumber ? typeof(N_PuzzleSolver) : typeof(Color_PuzzleSolver)))
                .Initialize(this, this, mShuffledList.ToArray(), GetPuzzle()).Next();
        }

        protected override bool CheckTheWin() => !mSolver.WillItBeContinued && mSolution.Count <= 0;

        protected override void OnDestroy() => StopAllCoroutines();
    }

    [Serializable]
    sealed class SerializationAI_GameBoard : SerializationGameBoard
    {
        public enum PuzzleSolverType { N_Solver, Color_Solver }

        public PuzzleSolverType SolverType;
        public SerializeblePuzzleSolver Solver;
        public List<SerializePath> Solution;
        public GameOptions.GameLevels Level;

        public static PuzzleSolverType GetEnumType<T>(T solver) where T : PuzzleSolver
        {
            if (solver is N_PuzzleSolver) return PuzzleSolverType.N_Solver;
            else if (solver is Color_PuzzleSolver) return PuzzleSolverType.Color_Solver;
            else throw new TypeAccessException();
        }

        public static Type GetPuzzleType(PuzzleSolverType solverType)
        {
            if (solverType == PuzzleSolverType.N_Solver) return typeof(N_PuzzleSolver);
            else if (solverType == PuzzleSolverType.Color_Solver) return typeof(Color_PuzzleSolver);
            else throw new TypeAccessException();
        }
    }

    [Serializable]
    struct SerializePath
    {
        public sbyte[] puzzle;

        public int step;

        public SerializebleVector2 moveTo;

        public byte direction;

        public static implicit operator SerializePath(Path path) => new SerializePath
        {
            puzzle = path.puzzle,
            step = path.step,
            moveTo = (SerializebleVector2) path.moveTo,
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