using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector2 = UnityEngine.Vector2Int;

namespace Assets.Scripts.PuzzleSolvers.SolverClasses
{
    public abstract class PuzzleSolver : IComparer<(Node, (int, int))>
    {
        protected sbyte[] mGoalState;

        protected Node mStartNode;
        protected MonoBehaviour mComponent;
        protected IAdapter mAdapter;
        protected sbyte mSize;
        protected bool mWillItBeContinued;

        public sbyte[] StartState { get => mStartNode.mPuzzle;
            set
            {
                mStartNode = new Node(value);
                mSize = (sbyte)Math.Sqrt(mStartNode.mPuzzle.Length);
            }
        }
        public bool WillItBeContinued => mWillItBeContinued;

        public PuzzleSolver() { }
        public PuzzleSolver(IAdapter adapter, MonoBehaviour component, sbyte[] puzzle = default, sbyte[] goal = default) =>
            Initialize(adapter, component, puzzle, goal);

        public virtual PuzzleSolver Initialize(IAdapter adapter, MonoBehaviour component, sbyte[] puzzle, sbyte[] goal)
        {
            mComponent = component;
            mAdapter = adapter;
            mGoalState = goal;
            mWillItBeContinued = true;
            if (puzzle == null) puzzle = new sbyte[(int)Math.Pow((int)GameOptions.MinSizeOfSquare, 2)];

            mStartNode = new Node(puzzle);
            mSize = (sbyte)Math.Sqrt(puzzle.Length);
            return this;
        }


        public virtual bool Next()
        {
            if (!mWillItBeContinued) return mWillItBeContinued;

            mComponent.StartCoroutine(IsSolve(mStartNode));

            return mWillItBeContinued;
        }

        protected IEnumerator IsSolve(Node root)
        {
            var currentNode = root;
            Calculate(currentNode);

            if (IsGoal(currentNode))
            {
                PathFound(currentNode);
                yield break;
            }

            var openList = new List<(Node, (int, int) priority)>();
            var closeList = new List<(Node, (int, int) priority)>
            { (currentNode, (currentNode.mCost, currentNode.mStep)) };

            do {

                foreach (var currentChild in currentNode.ExpandNode())
                {
                    if (IsGoal(currentChild))
                    {
                        mWillItBeContinued = false;
                        PathFound(currentChild);
                        yield break;
                    }
                    else if (!Contains(openList, currentChild) && !Contains(closeList, currentChild))
                    {
                        Calculate(currentChild);
                        openList.Add((currentChild, (currentChild.mCost, currentChild.mStep)));
                        openList.Sort(this);
                    }
                }

                if (openList.Count == 0) break;
                var current = openList[0];
                openList.RemoveAt(0);
                closeList.Add(current);
                currentNode = current.Item1;

                yield return null;

            } while (openList.Count >= 0);

            mWillItBeContinued = false;
        }

        int IComparer<(Node, (int, int))>.Compare((Node, (int, int)) x, (Node, (int, int)) y)
        {
            if (y.Item2.Item1 - x.Item2.Item1 > 0) return -1;
            else if (y.Item2.Item1 - x.Item2.Item1 < 0) return 1;
            else if (y.Item2.Item2 - x.Item2.Item2 > 0) return -1;
            else if (y.Item2.Item2 - x.Item2.Item2 < 0) return 1;
            else return 0;
        }

        protected virtual void PathFound(Node goalNode)
        {
            mStartNode = new Node(goalNode.mPuzzle);

            if (mAdapter == null) return;

            var locations = goalNode.GetPath().ConvertAll(
                p => new Path() { puzzle = p.mPuzzle, step = p.mStep, moveTo = p.mEmptyIndex.ConvertIndexToVector(mSize), direction = p.mDirection });
            mAdapter.FoundSolution(locations);
        }

        protected virtual bool Contains(List<(Node, (int, int))> queue, Node target)
        {
            foreach (var (child, (_, _)) in queue)
                if (target.Equals(child)) return true;
            return false;
        }

        protected abstract bool IsGoal(Node node);

        protected abstract void Calculate(Node node);
    }

    public struct Path
    {
        public sbyte[] puzzle;
        public int step;
        public Vector2 moveTo;
        public Direction direction;
    }
}
