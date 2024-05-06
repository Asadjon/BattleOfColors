using System;
using UnityEngine;

namespace Assets.Scripts.PuzzleSolvers.SolverClasses
{
    public static class Extensions
    {
        public static PuzzleSolver NewInstate(this Type solverType)
        {
            if (!solverType.IsSubclassOf(typeof(PuzzleSolver))) return null;

            return (PuzzleSolver)Activator.CreateInstance(solverType);
        }

        public static T NewInstate<T>() where T : PuzzleSolver =>
            (T) typeof(T).NewInstate();
    }
}
