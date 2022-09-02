using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.PuzzleSolvers.SolverClasses
{
    public sealed class N_PuzzleSolver : PuzzleSolver
    {
        private sbyte[] mPartOfGoal;
        private sbyte[] SetPartOfGoal
        {
            set
            {
                var i = 0;
                while (i < value.Length && !mPartOfGoal.Contains(value[i]))
                {
                    Array.Resize(ref mPartOfGoal, mPartOfGoal.Length + 1);
                    mPartOfGoal[mPartOfGoal.Length - 1] = value[i];
                    i++;
                }
            }
        }
        private sbyte mLastTarget = 0;
        private sbyte[][] mPattern;
        private Vector2Int mPatternIndex;

        public N_PuzzleSolver() { }
        public N_PuzzleSolver(IAdapter adapter, MonoBehaviour component, sbyte[] puzzle, sbyte[] goal) : base(adapter, component, puzzle, goal) { }

        public override PuzzleSolver Initialize(IAdapter adapter, MonoBehaviour component, sbyte[] puzzle, sbyte[] goal)
        {
            base.Initialize(adapter, component, puzzle, goal);
            mPattern = PatternDatabase.mDatabases[mSize - GameOptions.MinNumberOfArrays];
            mPatternIndex = Vector2Int.zero;
            mPartOfGoal = new sbyte[0];
            SetPartOfGoal = mPattern[mPatternIndex.y];

            return this;
        }

        public override bool Next()
        {
            if (!mWillItBeContinued) return mWillItBeContinued;

            mLastTarget = mPattern[mPatternIndex.y][mPatternIndex.x];

            mComponent.StartCoroutine(IsSolve(mStartNode));

            return mWillItBeContinued;
        }

        protected override void PathFound(Node goalNode)
        {
            mWillItBeContinued = true;
            if (mPatternIndex.x + 1 < mPattern[mPatternIndex.y].Length) mPatternIndex.x++;
            else if (mPatternIndex.y + 1 < mPattern.Length)
            {
                mPatternIndex.Set(0, mPatternIndex.y + 1);
                SetPartOfGoal = mPattern[mPatternIndex.y];
            }
            else mWillItBeContinued = false;

            base.PathFound(goalNode);
        }

        protected override bool IsGoal(Node node)
        {
            for (ushort i = 0; i <= Array.IndexOf(mPartOfGoal, mLastTarget); i++)
                if (Array.IndexOf(mGoalState, mPartOfGoal[i]) != Array.IndexOf(node.mPuzzle, mPartOfGoal[i]))
                    return false;
            return true;
        }

        protected override void Calculate(Node node) =>
            node.mCost = (ushort)(GetManhattanDistance(node.mPuzzle) + GetLinearConflict(node.mPuzzle) * 2 + GetDistance(node.mPuzzle, node.mEmptyIndex) * 2);

        public ushort GetLinearConflict(sbyte[] puzzle)
        {
            ushort linearConflict = 0;
            var max = Vector2.one * -1;

            // horizontal
            for (int i = 0; i < mGoalState.Length - 2; i++)
            {
                var goalPos = i.ConvertIndexToVector(mSize);

                if (goalPos == Vector2.up * goalPos) max = Vector2.one * -1;
                if (!mPartOfGoal.Contains(mGoalState[i])) continue;
                var pos = Array.IndexOf(puzzle, mGoalState[i]).ConvertIndexToVector(mSize);

                if (goalPos.y == pos.y)
                {
                    if (pos.x > max.x) max = pos;
                    else linearConflict++;
                }
            }

            // vertical
            for (int i = 0; i < mGoalState.Length - 2; i++)
            {
                var goalPos = i.ConvertIndexToVector(mSize);
                goalPos = new Vector2Int(goalPos.y, goalPos.x);
                var goalValue = mGoalState[goalPos.y * mSize + goalPos.x];

                if (goalPos == Vector2.right * goalPos) max = Vector2.one * -1;
                if (!mPartOfGoal.Contains(goalValue)) continue;
                var pos = Array.IndexOf(puzzle, goalValue).ConvertIndexToVector(mSize);

                if (goalPos.x == pos.x)
                {
                    if (pos.y > max.y) max = pos;
                    else linearConflict++;
                }
            }

            return linearConflict;
        }

        public ushort GetManhattanDistance(sbyte[] puzzle)
        {
            ushort manhattnDistance = 0;
            for (int i = 0; i < mGoalState.Length - 2; i++)
            {
                if (!mPartOfGoal.Contains(mGoalState[i])) continue;

                var goalPos = i.ConvertIndexToVector(mSize);
                var pos = Array.IndexOf(puzzle, mGoalState[i]).ConvertIndexToVector(mSize);

                var summ = (goalPos - pos).Abs();
                manhattnDistance += (ushort)(summ.x + summ.y);
            }

            return manhattnDistance;
        }

        public ushort GetDistance(sbyte[] puzzle, int emptyIndex)
        {
            var emptyPos = emptyIndex.ConvertIndexToVector(mSize);
            var targetPos = Array.IndexOf(puzzle, mLastTarget).ConvertIndexToVector(mSize);
            var direction = (targetPos - emptyPos).Abs();
            return (ushort)(direction.x + direction.y - 1);
        }

        protected override bool Contains(List<(Node, (int, int))> queue, Node node)
        {
            var targets = mPartOfGoal.ToDictionary(p => Array.IndexOf(node.mPuzzle, p), p => p);
            targets.Add(node.mEmptyIndex, 0);

            foreach (var (child, (_, _)) in queue)
            {
                var equal = true;
                foreach (var target in targets)
                    if (child.mPuzzle[target.Key] != target.Value)
                    {
                        equal = false;
                        break;
                    }
                if (equal) return true;
            }

            return false;
        }

        protected override SerializeblePuzzleSolver GetMyPuzzleSolver() =>
            new SerializebleN_PuzzleSolver { PatternIndex = (SerializebleVector2) mPatternIndex, PartOfGoal = mPartOfGoal, LastTarget = mLastTarget };

        public override PuzzleSolver Set(SerializeblePuzzleSolver solver)
        {
            base.Set(solver);
            mPattern = PatternDatabase.mDatabases[mSize - GameOptions.MinNumberOfArrays];
            if (!(solver is SerializebleN_PuzzleSolver n_solver)) return this;

            mPartOfGoal = n_solver.PartOfGoal;
            mPatternIndex = (Vector2Int)n_solver.PatternIndex;
            mLastTarget = n_solver.LastTarget;

            return this;
        }
    }

    [Serializable] class SerializebleN_PuzzleSolver : SerializeblePuzzleSolver
    {
        public SerializebleVector2 PatternIndex;
        public sbyte[] PartOfGoal;
        public sbyte LastTarget;
    }

    internal static class PatternDatabase
    {
        internal readonly static sbyte[][] mDatabase_3x3 = new sbyte[][]
        {   new sbyte[] { 1, 2, 3, 4, 7 },
            new sbyte[] { 5, 6, 8 } };

        internal readonly static sbyte[][] mDatabase_4x4 = new sbyte[][]
        {   new sbyte[] { 1, 2, 3, 4, 5, 9, 13 },
            new sbyte[] { 6, 7, 8, 10, 14 },
            new sbyte[] { 11, 12, 15 } };

        internal readonly static sbyte[][] mDatabase_5x5 = new sbyte[][]
        {   new sbyte[] { 1, 2, 3, 4, 5, 6, 11, 16, 21 },
            new sbyte[] { 7, 8, 9, 10, 12, 17, 22 },
            new sbyte[] { 13, 14, 15, 18, 23 },
            new sbyte[] { 19, 20, 24 } };

        internal readonly static sbyte[][] mDatabase_6x6 = new sbyte[][]
        {   new sbyte[] { 1, 2, 3, 4, 5, 6, 7, 13, 19, 25, 31 },
            new sbyte[] { 8, 9, 10, 11, 12, 14, 20, 26, 32 },
            new sbyte[] { 15, 16, 17, 18, 21, 27, 33 },
            new sbyte[] { 22, 23, 24, 28, 34 },
            new sbyte[] { 29, 30, 35 } };

        internal readonly static sbyte[][] mDatabase_7x7 = new sbyte[][]
        {   new sbyte[] { 1, 2, 3, 4, 5, 6, 7, 8, 15, 22, 29, 36, 43 },
            new sbyte[] { 9, 10, 11, 12, 13, 14, 16, 23, 30, 37, 44 },
            new sbyte[] { 17, 18, 19, 20, 21, 24, 31, 38, 45 },
            new sbyte[] { 25, 26, 27, 28, 32, 39, 46 },
            new sbyte[] { 33, 34, 35, 40, 47 },
            new sbyte[] { 41, 42, 48 } };

        internal readonly static sbyte[][] mDatabase_8x8 = new sbyte[][]
        {   new sbyte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 17, 25, 33, 41, 49, 57 },
            new sbyte[] { 10, 11, 12, 13, 14, 15, 16, 18, 26, 34, 42, 50, 58 },
            new sbyte[] { 19, 20, 21, 22, 23, 24, 27, 35, 43, 51, 59 },
            new sbyte[] { 28, 29, 30, 31, 32, 36, 44, 52, 60 },
            new sbyte[] { 37, 38, 39, 40, 45, 53, 61 },
            new sbyte[] { 46, 47, 48, 54, 62 },
            new sbyte[] { 55, 56, 63 } };

        internal readonly static sbyte[][][] mDatabases = new sbyte[][][] { mDatabase_3x3, mDatabase_4x4, mDatabase_5x5, mDatabase_6x6, mDatabase_7x7, mDatabase_8x8 };
    }
}
