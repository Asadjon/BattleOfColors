using Assets.Scripts.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using URand = UnityEngine.Random;

namespace Assets.Scripts
{
    static class MyStaticHelper
    {
        #region Shuffle
        public static List<T> Shuffle<T>(this List<T> nodeList)
        {
            var list = new List<T>(nodeList);
            var n = nodeList.Count;

            while (n > 1) list.Swap(URand.Range(0, n--), n);

            return list;
        }
        public static void Swap<T>(this List<T> list, int indexA, int indexB) =>
            (list[indexB], list[indexA]) = (list[indexA], list[indexB]);

        public static List<sbyte> ShuffleIsSolvable(this sbyte[] puzzle, bool isFifteen)
        {
            var list = puzzle.ToList();

            var shuffledList = list.Shuffle();
            while (isFifteen && !IsSolvable(shuffledList = list.Shuffle()));

            return shuffledList;
        }
        private static bool IsSolvable(List<sbyte> puzzle)
        {
            // Count inversions in given puzzle
            var invCount = GetInverseCount(puzzle);

            // If grid is odd, return true if inversion
            // count is even.
            if (((int)Math.Sqrt(puzzle.Count)) % 2 == 1) return invCount % 2 == 0;
            else return FindEmptyXPosition(puzzle) % 2 == 1 ? invCount % 2 == 0 : invCount % 2 == 1;
        }
        private static int GetInverseCount(List<sbyte> puzzle)
        {
            var inverseCount = 0;

            for (var i = 0; i < puzzle.Count - 1; i++)
                for (var j = i + 1; j < puzzle.Count; j++)
                    if (puzzle[i] != 0 && puzzle[j] != 0 && puzzle[i] > puzzle[j])
                        inverseCount++;

            return inverseCount;
        }
        private static int FindEmptyXPosition(List<sbyte> puzzle) =>
            ((int) Math.Sqrt(puzzle.Count)) - puzzle.IndexOf(0) / (int)Math.Sqrt(puzzle.Count);
        #endregion

        public static ViewResource ChangeViewResource(this ViewResource res, int count, int i) =>
            res.Set(i + 1, (i / count + 1).ToString(), res.Color);
    }
}
