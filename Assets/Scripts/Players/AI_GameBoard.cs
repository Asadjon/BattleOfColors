using Assets.Scripts.PuzzleSolvers;
using Assets.Scripts.PuzzleSolvers.PuzzleEditor;
using Assets.Scripts.PuzzleSolvers.SolverClasses;
using Assets.Scripts.Records;
using Assets.Scripts.Resource;
using Assets.Scripts.SaveGameDatas.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Assets.Scripts.GameOptions;

namespace Assets.Scripts.Players
{
	[Serialization(typeof(SerializationAI_GameBoard))]
	sealed class AI_GameBoard : GameBoard, IAdapter
	{
		private PuzzleSolver mSolver;
		private readonly List<Path> mSolution = new List<Path>();
		private bool mIsRun;
		private GameSettings mSettings;
		private GameOptions mOptions;

		private float mItemSwipeTime = 0;
		private float mCurrentTime = 0;

		protected override void Set_GameType(GameTypes gameType)
		{
			base.Set_GameType(gameType);
            mItemSwipeTime = GetLevelValue();
        }

		private List<SerializePath> Solution
		{
			get => mSolution.ConvertAll(p => (SerializePath)p);
			set
			{
				if (value == null) return;
				mSolution.Clear();
				mSolution.AddRange(value.ConvertAll(p => (Path)p));
			}
        }

        protected override void LoadData()
		{
			base.LoadData();
			mSettings = GameSettings.Instance;
			mOptions = GameOptions.Instance;
		}

		private float GetLevelValue()
        {
            var average = AveragePathsCount.Instance.Lerp(mGameType, mGameLevel, mSizeOfSquare, .5f)/* * 2*/;
            return (float)mOptions.RecordData.GetAverage(RecordData.Parametrs.Time) / average;
        }

		int countAllSolutions;
		void IAdapter.FoundSolution(List<Path> solution)
		{
			mSolution.AddRange(solution);

			countAllSolutions += PathCounter(solution.Select(path => path.direction));

			if (!mSolver.Next() && countAllSolutions != -1 && solution.Count > 0)
			{
				print(countAllSolutions.ToString());
				AveragePathsCount.Instance.Add(mGameType, mGameLevel, mSizeOfSquare, countAllSolutions);
                print(AveragePathsCount.Instance.Lerp(mGameType, mGameLevel, mSizeOfSquare, .5f).ToString());
                countAllSolutions = -1;
            }
        }

		private int PathCounter(IEnumerable<Direction> solution)
		{
			if (solution == null || solution.Count() == 0) return 0;

			var dir = solution.First();
            return solution.Count(dir1 =>
            {
				var isNotEqual = dir != dir1;
                dir = dir1;
				return isNotEqual;
            });
        }

		private void Update()
		{
			var checkedTheWin = CheckTheWin();
			if (mIsRun && !checkedTheWin)
			{
				mCurrentTime += Time.deltaTime;
				if (Run()) mCurrentTime = 0;
			}
			else if (mIsRun && checkedTheWin) GameOver();
		}
		private bool Run()
		{
			if (mCurrentTime >= mItemSwipeTime && GetPath(out Path step))
			{
				var distanceOfSwiping = mSettings.DistanceOfSwiping;
				for (var i = 1; i < distanceOfSwiping; i++)
					if (mSolution.Count == 0 || step.direction != mSolution.First().direction || !GetPath(out step)) break;

				SwitchSpecifiedPositions(step.moveTo - mEmptyNode.PositionInTheArray);

				return true;
			}
			return false;
		}

		private bool GetPath(out Path path)
		{
			if (mSolution.Count > 0)
			{
				path = mSolution.First();
				mSolution.RemoveAt(0);
				return true;
			}

			path = default;
			return false;
		}

		public override void PauseGame() => mIsRun = false;

		public override void PlayGame() => mIsRun = true;

		public override void StopGame()
		{
			base.StopGame();
			mSolution.Clear();
		}

		public override void Restart(sbyte[] shuffle)
		{
			base.Restart(shuffle);
            ReInitSolver();
		}

		private void ReInitSolver()
		{
			countAllSolutions = 0;
            StopAllCoroutines();
			var startState = mSolution.Count == 0 ? mShuffledList : mSolution.Last().puzzle;
            (mSolver = GetSolverType().NewInstate().Initialize(this, this, startState, mGoalState)).Next();
		}

        private Type GetSolverType() =>
			GameType == GameTypes.WithNumber ? typeof(N_PuzzleSolver) : typeof(Color_PuzzleSolver);

        private bool CheckTheWin() => (mSolver == null || !mSolver.WillItBeContinued) && mSolution.Count <= 0;

		protected override void OnDestroy() => StopAllCoroutines();
	}

	#region Serialize game datas

	[Serializable]
	sealed class SerializationAI_GameBoard : SerializationGameBoard
	{
		[SerializedMember("Solution")] public List<SerializePath> Solution;
	}

	[Serializable]
	struct SerializePath
	{
		public sbyte[] puzzle;

		public int step;

		public SerializableVector2Int moveTo;

		public byte direction;

		public static implicit operator SerializePath(Path path) => new SerializePath
		{
			puzzle = path.puzzle,
			step = path.step,
			moveTo = (SerializableVector2Int) path.moveTo,
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
	#endregion
}