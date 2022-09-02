using Assets.Scripts.PuzzleSolvers;
using System;
using System.Collections.Generic;
using Vector2 = UnityEngine.Vector2Int;

namespace Assets.Scripts.PuzzleSolvers
{
    public class Node
    {
        internal sbyte[] mPuzzle;
        internal int mCost;
        internal int mStep;
        internal readonly int mSize;
        internal int mEmptyIndex;

        internal Direction mDirection;
        internal Node mParent;
        private static readonly Vector2[] mDirections = new[] { Vector2.left, Vector2.right, Vector2.up, Vector2.down };

        private readonly int mDidNotMove;

        public Node(sbyte[] puzzle) : this(puzzle, null, Direction.Default, 0) { }

        protected Node(sbyte[] puzzle, Node parent, Direction direction, ushort step)
        {
            mParent = parent;
            mDirection = direction;
            mStep = step;
            mPuzzle = puzzle;
            mSize = (int)(mPuzzle.Length > 0 ? Math.Sqrt(mPuzzle.Length) : 0);
            mEmptyIndex = Array.IndexOf(mPuzzle, (sbyte)0);
            if (mParent) mDidNotMove = (ushort)Array.IndexOf(mParent.mPuzzle, (sbyte)0);
        }

        public List<Node> ExpandNode()
        {
            var priorityQueue = new List<Node>();

            for (sbyte i = 0; i < mDirections.Length; i++)
            {
                var child = MoveTo(mDirections[i], (Direction) i);
                if (!child) continue;
                priorityQueue.Add(child);
            }

            return priorityQueue;
        }

        private Node MoveTo(Vector2 dir, Direction direction)
        {
            var emptyPos = mEmptyIndex.ConvertIndexToVector(mSize);
            var to = emptyPos + dir;
            if (mDidNotMove.ConvertIndexToVector(mSize) == to || to.x > mSize - 1 || to.y > mSize - 1 || to.x < 0 || to.y < 0) return null;

            var clonePuzzle = (sbyte[])mPuzzle.Clone();
            clonePuzzle.Swap(mEmptyIndex, (ushort)(to.y * mSize + to.x));

            return new Node(clonePuzzle, this, direction, (ushort) (mStep + 1));
        }

        public List<Node> GetPath()
        {
            var path = new List<Node>();
            if (!mParent) return path;

            path.AddRange(mParent.GetPath());
            path.Add(this);
            return path;
        }

        public static void Print(Node[] solution)
        {
            for (ushort i = 0; i < solution?.Length; i++)
                Console.WriteLine(solution[i]);
        }

        public override int GetHashCode() => base.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Node node)) return false;
            return Equals(node.mPuzzle);
        }

        public bool Equals(sbyte[] puzzle)
        {
            if (puzzle.Length == 0) return false;
            for (ushort i = 0; i < puzzle.Length; i++)
            {
                if (puzzle[i] != mPuzzle[i])
                    return false;
            }
            return true;
        }

        public override string ToString()
        {
            var puzzleString = "\tStep: " + mStep + "\n";

            Array.ForEach(mPuzzle, p =>
            {
                puzzleString += (p == 0 ? "_" : p.ToString()) + " ";
                if (Array.IndexOf(mPuzzle, p).ConvertIndexToVector(mSize).x >= mSize - 1) puzzleString += "\n";
            });

            return puzzleString;
        }

        public static bool operator ==(Node a, Node b) => Equals(a, b);
        public static bool operator !=(Node a, Node b) => !Equals(a, b);
        public static bool operator !(Node a) => a == null;
        public static bool operator true(Node a) => a != null;
        public static bool operator false(Node a) => a == null;
    }
}
