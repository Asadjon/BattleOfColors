using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector2 = UnityEngine.Vector2Int;

namespace Assets.Scripts.PuzzleSolvers.SolverClasses
{
    public abstract class PuzzleSolver : IComparer<(Node, (int, int))>
    {
        private const int YieldInterval = 128;

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
            puzzle ??= new sbyte[(int)Math.Pow((int)GameOptions.MinSizeOfSquare, 2)];

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

            var openList = new NodePriorityHeap(this);
            var openKeys = new HashSet<string>();
            var closedKeys = new HashSet<string> { GetStateKey(currentNode) };
            var iterations = 0;

            do
            {

                foreach (var currentChild in currentNode.ExpandNode())
                {
                    if (IsGoal(currentChild))
                    {
                        mWillItBeContinued = false;
                        PathFound(currentChild);
                        yield break;
                    }
                    var childKey = GetStateKey(currentChild);
                    if (closedKeys.Contains(childKey) || openKeys.Contains(childKey))
                        continue;

                    Calculate(currentChild);
                    openList.Push(currentChild);
                    openKeys.Add(childKey);
                }

                if (openList.Count == 0) break;

                do
                {
                    currentNode = openList.Pop();
                    openKeys.Remove(GetStateKey(currentNode));
                }
                while (openList.Count > 0 && closedKeys.Contains(GetStateKey(currentNode)));

                closedKeys.Add(GetStateKey(currentNode));

                if (++iterations % YieldInterval == 0)
                    yield return null;

            } while (openList.Count > 0);

            mWillItBeContinued = false;
        }

        protected virtual string GetStateKey(Node node)
        {
            var key = new char[node.mPuzzle.Length];
            for (var i = 0; i < node.mPuzzle.Length; i++)
                key[i] = (char)(node.mPuzzle[i] + 1);

            return new string(key);
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

        private sealed class NodePriorityHeap
        {
            private readonly IComparer<(Node, (int, int))> mComparer;
            private readonly List<Node> mNodes = new();

            public NodePriorityHeap(IComparer<(Node, (int, int))> comparer)
            {
                mComparer = comparer;
            }

            public int Count => mNodes.Count;

            public void Push(Node node)
            {
                mNodes.Add(node);

                var index = mNodes.Count - 1;
                while (index > 0)
                {
                    var parentIndex = (index - 1) / 2;
                    if (Compare(mNodes[index], mNodes[parentIndex]) >= 0)
                        break;

                    Swap(index, parentIndex);
                    index = parentIndex;
                }
            }

            public Node Pop()
            {
                var result = mNodes[0];
                var last = mNodes[^1];
                mNodes.RemoveAt(mNodes.Count - 1);

                if (mNodes.Count == 0)
                    return result;

                mNodes[0] = last;
                Heapify(0);
                return result;
            }

            private void Heapify(int index)
            {
                while (true)
                {
                    var left = index * 2 + 1;
                    var right = left + 1;
                    var smallest = index;

                    if (left < mNodes.Count && Compare(mNodes[left], mNodes[smallest]) < 0)
                        smallest = left;

                    if (right < mNodes.Count && Compare(mNodes[right], mNodes[smallest]) < 0)
                        smallest = right;

                    if (smallest == index)
                        return;

                    Swap(index, smallest);
                    index = smallest;
                }
            }

            private int Compare(Node x, Node y) =>
                mComparer.Compare((x, (x.mCost, x.mStep)), (y, (y.mCost, y.mStep)));

            private void Swap(int a, int b) => (mNodes[b], mNodes[a]) = (mNodes[a], mNodes[b]);
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
