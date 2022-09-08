using Assets.Scripts.PuzzleSolvers.SolverClasses;
using System.Collections.Generic;

namespace Assets.Scripts.PuzzleSolvers
{
    public interface IAdapter
    {
        void FoundSolution(List<Path> solution);
    }
}
