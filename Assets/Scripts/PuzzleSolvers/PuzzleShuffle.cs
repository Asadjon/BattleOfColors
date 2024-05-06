using System.Collections.Generic;
using System.Linq;
using static Assets.Scripts.GameOptions;
using URand = UnityEngine.Random;

namespace Assets.Scripts.PuzzleSolvers
{
    internal static class PuzzleShuffle
    {
        private static readonly Dictionary<GameLevels, int> mShuffleLevels = new Dictionary<GameLevels, int>
        { { GameLevels.Easy, 10 }, { GameLevels.Normal, 23 }, { GameLevels.Hard, 47 }, { GameLevels.Expert, 80 } };

        public static List<T> Shuffle<T>(this List<T> nodeList)
        {
            var list = new List<T>(nodeList);
            var n = nodeList.Count;

            while (n > 1) list.Swap(URand.Range(0, n--), n);

            return list;
        }

        public static sbyte[] Shuffle(this sbyte[] shufflingList, GameLevels level, int sizeIndex)
        {
            var root = new Node(shufflingList.ToArray());
            var shuffleCount = mShuffleLevels[level] * (sizeIndex + 1);

            for (int i = 0; i < shuffleCount; i++)
            {
                Node movedNode;
                while (true)
                {
                    int direction = URand.Range(0, Node.mDirections.Length);
                    if ((movedNode = root.MoveTo(Node.mDirections[direction], (Direction)direction)) != null) break;
                }

                root = movedNode;
            }

            return root.mPuzzle;
        }

        //private static bool IsSolvable(this List<sbyte> puzzle)
        //{
        //    // Count inversions in given puzzle
        //    var invCount = puzzle.GetInverseCount();

        //    // If grid is odd, return true if inversion
        //    // count is even.
        //    if (((int)Math.Sqrt(puzzle.Count)) % 2 == 1) return invCount % 2 == 0;
        //    else return FindEmptyXPosition(puzzle) % 2 == 1 ? invCount % 2 == 0 : invCount % 2 == 1;
        //}

        //private static int GetInverseCount(this List<sbyte> puzzle)
        //{
        //    var inverseCount = 0;

        //    for (var i = 0; i < puzzle.Count - 1; i++)
        //        for (var j = i + 1; j < puzzle.Count; j++)
        //            if (puzzle[i] != 0 && puzzle[j] != 0 && puzzle[i] > puzzle[j])
        //                inverseCount++;

        //    return inverseCount;
        //}

        //private static int FindEmptyXPosition(List<sbyte> puzzle) =>
        //    ((int)Math.Sqrt(puzzle.Count)) - puzzle.IndexOf(0) / (int)Math.Sqrt(puzzle.Count);
    }
}
