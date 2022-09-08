using Assets.Scripts.PuzzleSolvers.SolverClasses;
using System;
using System.Collections.Generic;
using Vector2 = UnityEngine.Vector2Int;

namespace Assets.Scripts.PuzzleSolvers
{
    public static class ProjectHelper
    {
        internal static Vector2 ConvertIndexToVector(this int index, int size) =>
            index % size * Vector2.right + index / size * Vector2.up;

        internal static void Swap<T>(this T[] array, int a, int b) =>
            (array[a], array[b]) = (array[b], array[a]);

        internal static void Swap<T>(this List<T> list, int indexA, int indexB) =>
            (list[indexB], list[indexA]) = (list[indexA], list[indexB]);

        internal static Vector2 Abs(this Vector2 vector) => new Vector2(Math.Abs(vector.x), Math.Abs(vector.y));

        public static string PathToString(this Path path)
        {
            var puzzleString = "\tStep: " + path.step + ", Move to: (" + path.moveTo.x + ", " + path.moveTo.y + ")\n";
            var size = (sbyte) Math.Sqrt(path.puzzle.Length);

            Array.ForEach(path.puzzle, p =>
            {
                puzzleString += (p == 0 ? "_" : p.ToString()) + " ";
                if (Array.IndexOf(path.puzzle, p).ConvertIndexToVector(size).x >= size - 1) puzzleString += "\n";
            });

            return puzzleString;
        }
    }
}
