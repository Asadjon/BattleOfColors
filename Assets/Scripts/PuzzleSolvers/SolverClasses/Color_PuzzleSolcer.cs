using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.PuzzleSolvers.SolverClasses
{
    public sealed class Color_PuzzleSolver : PuzzleSolver
    {
        private sbyte[] mPartOfGoal;
        private sbyte PartIndex
        {
            get => mPartIndex;
            set
            {
                mPartIndex = value;
                mPartOfGoal = new sbyte[mSize];
                for (ushort i = 0; i < mPartOfGoal.Length; i++)
                    mPartOfGoal[i] = i <= mPartIndex ? mGoalState[i] : (sbyte)-1;
            }
        }
        private sbyte mPartIndex;

        public Color_PuzzleSolver() { }
        public Color_PuzzleSolver(IAdapter adapter, MonoBehaviour component, sbyte[] puzzle, sbyte[] goal) : base(adapter, component, puzzle, goal) { }

        public override PuzzleSolver Initialize(IAdapter adapter, MonoBehaviour component, sbyte[] puzzle, sbyte[] goal)
        {
            base.Initialize(adapter, component, ConvertColorPuzzle(puzzle), ConvertColorPuzzle(goal));
            PartIndex = 0;
            return this;
        }

        protected override void PathFound(Node goalNode)
        {
            mWillItBeContinued = false;
            if (PartIndex + 1 < mSize)
            {
                mWillItBeContinued = true;
                PartIndex = ++PartIndex;
            }

            base.PathFound(goalNode);
        }

        protected override void Calculate(Node node) =>
            node.mCost = (ushort)(GetManhattanDistance(node.mPuzzle) + GetLinearConflict(node.mPuzzle) * 2);

        protected override bool IsGoal(Node node)
        {
            for (var i = 0; i <= PartIndex; i++) for (var j = 0; j < mSize; j++)
                {
                    var index = j * mSize + i;
                    if (mGoalState[index] != node.mPuzzle[index]) return false;
                }

            return true;
        }

        public ushort GetLinearConflict(sbyte[] puzzle)
        {
            ushort linearConflict = 0;
            var max = -1;

            for (int i = 0; i < puzzle.Length; i++)
            {
                if (i.ConvertIndexToVector(mSize).x == 0) max = -1;
                if (puzzle[i] == 0) continue;

                var partIndex = Array.IndexOf(mPartOfGoal, puzzle[i]);

                if (partIndex == -1 || partIndex >= max) max = partIndex;
                else linearConflict++;
            }

            return linearConflict;
        }

        public ushort GetManhattanDistance(sbyte[] puzzle)
        {
            ushort manhattnDistance1 = 0;

            for (int i = 0; i < PartIndex; i++) for (int j = 0; j < puzzle.Length; j++)
                    if (puzzle[j] == mPartOfGoal[i])
                        manhattnDistance1 += (ushort)Mathf.Abs(i - j.ConvertIndexToVector(mSize).x);

            ushort manhattnDistance2 = 0;
            for (int j = 0; j < puzzle.Length; j++)
                    if (puzzle[j] == mPartOfGoal[PartIndex])
                        manhattnDistance2 += (ushort)Mathf.Abs(PartIndex - j.ConvertIndexToVector(mSize).x);

            return (ushort)(manhattnDistance1 + manhattnDistance2 * 3);
        }

        protected override bool Contains(List<(Node, (int, int))> queue, Node note)
        {
            var targets = new Dictionary<int, sbyte>
            { { note.mEmptyIndex, 0 } };

            for (ushort i = 0; i < note.mPuzzle.Length; i++)
                if (mPartOfGoal.Contains(note.mPuzzle[i]))
                    targets.Add(i, note.mPuzzle[i]);

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

        private sbyte[] ConvertColorPuzzle(sbyte[] puzzle)
        {
            if (puzzle == null) return null;

            sbyte size = (sbyte)Math.Sqrt(puzzle.Length);
            var colorPuzzle = new sbyte[puzzle.Length];
            for (ushort i = 0; i < puzzle.Length; i++)
            {
                int value = puzzle[i];
                colorPuzzle[i] = (sbyte)(value == 0 ? 0 : value.ConvertIndexToVector(size).x == 0 ? size : value.ConvertIndexToVector(size).x);
            }
            return colorPuzzle;
        }
    }
}
