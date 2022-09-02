using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Players
{
    internal class SortCalculation
    {
        #region With Color
        //    private static SortCalculation sortCalculation;
        //    private byte[][] _workNumbers;
        //    private int _numberOfArrays;
        //    public readonly Vector2 _emptyColorPosition = Vector2.zero;
        //    private readonly List<Step> _botSteps = new List<Step>();
        //    private List<byte> _indicatorColors;

        //    public static SortCalculation Init() =>
        //        sortCalculation != null ? sortCalculation : (sortCalculation = new SortCalculation());

        //    public SortCalculation Sort(byte[][] mCordinates, List<byte> indicatorColors, int numberOfArrays)
        //    {
        //        _numberOfArrays = numberOfArrays;
        //        _workNumbers = new byte[_numberOfArrays + 1][/*_numberOfArrays + 1*/];
        //        int x, y;
        //        for (int i = 0; i < Mathf.Pow(_numberOfArrays, 2); i++)
        //        {
        //            x = i % _numberOfArrays + 1;
        //            y = i / _numberOfArrays + 1;
        //            _workNumbers[y][x] = mCordinates[y - 1][x - 1];
        //        }
        //        _indicatorColors = new List<byte>(indicatorColors);
        //        _botSteps.Clear();
        //        Solve();

        //        return this;
        //    }

        //private void Run(int dy, int dx)
        //    {
        //        Vector2 _changeColorPosition = new Vector2(_emptyColorPosition.x + dx, _emptyColorPosition.y + dy);
        //        _botSteps.Add(new Step(_emptyColorPosition, _changeColorPosition));

        //        _workNumbers[(int)_emptyColorPosition.y][(int)_emptyColorPosition.x] = _workNumbers[(int)_changeColorPosition.y][(int)_changeColorPosition.x];
        //        _workNumbers[(int)_changeColorPosition.y][(int)_changeColorPosition.x] = 0;

        //        StringBuilder message = new StringBuilder();
        //        message.Append(string.Format("(%2d, %2d) -> (%2d, %2d)", new object[] { _emptyColorPosition.y, _emptyColorPosition.x, _changeColorPosition.y, _changeColorPosition.x })).Append("\n");
        //        _emptyColorPosition.Set(_changeColorPosition.x, _changeColorPosition.y);

        //        for (int i = 1; i <= _numberOfArrays; i++) { message.Append("----"); }

        //        message.Append("\n");
        //        for (int y = 1; y <= _numberOfArrays; y++)
        //        {
        //            for (int x = 1; x <= _numberOfArrays; x++)
        //            {

        //                if (y == this._emptyColorPosition.y && x == this._emptyColorPosition.x) message.Append("    ");
        //                else message.Append(_workNumbers[y][x]).Append("   ");
        //            }
        //            message.Append("\n");
        //        }

        //        Debug.Log(message.ToString());
        //    }

        //private void Solve()
        //    {
        //        int x, y;

        //        for (int i = 0; i < Mathf.Pow(_numberOfArrays, 2); i++)
        //        {
        //            x = i % _numberOfArrays + 1;
        //            y = i / _numberOfArrays + 1;
        //            if (_workNumbers[y][x] == 0)
        //            {
        //                _emptyColorPosition.Set(x, y);
        //                break;
        //            }
        //        }

        //        for (int i = 0; i < Mathf.Pow(_numberOfArrays, 2); i++)
        //        {
        //            x = i % _numberOfArrays + 1;
        //            y = i / _numberOfArrays + 1;

        //            if (y == _numberOfArrays && x == _numberOfArrays) { break; }
        //            while (_emptyColorPosition.y != y || _emptyColorPosition.x != x)
        //            {
        //                if ((y - 1) * _numberOfArrays + x <= (_emptyColorPosition.y - 2) * _numberOfArrays + _emptyColorPosition.x)
        //                    Run(-1, 0);
        //                else
        //                {
        //                    if (x > _emptyColorPosition.x) Run(0, 1); 
        //                    else Run(0, -1); 
        //                }
        //            }
        //            var u = y;
        //            var v = x;
        //            try
        //            {
        //                while (!Equals(_workNumbers[u][v], _indicatorColors[y - 1]))
        //                {
        //                    v++;
        //                    if (v > _numberOfArrays)
        //                    {
        //                        u++;
        //                        v = 1;
        //                    }
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                Debug.LogError(e.Message);
        //            }
        //            Play(u, v, (int)_emptyColorPosition.y, (int)_emptyColorPosition.x);
        //        }
        //    }

        //    private void Play(int y, int x, int emptyColor_Y, int emptyColor_X)
        //    {
        //        if (emptyColor_Y == y)
        //        {
        //            for (int i = 0; i < x - emptyColor_X; i++)
        //                Run(0, 1);
        //            x--;
        //            while (x != emptyColor_X)
        //            {
        //                Run(1, 0);
        //                Run(0, -1);
        //                Run(0, -1);
        //                Run(-1, 0);
        //                Run(0, 1);
        //                x--;
        //            }
        //            return;
        //        }
        //        if (emptyColor_Y == y - 1 && x == emptyColor_X)
        //        {
        //            Run(1, 0);
        //            return;
        //        }
        //        if (emptyColor_X == 1)
        //        {
        //            for (int i = 1; i < y - emptyColor_Y; i++)
        //                Run(1, 0);
        //            if (x != 1)
        //            {
        //                Run(1, 0);
        //                for (int i = 2; i < x; i++) Run(0, 1);

        //                while (x != 2)
        //                {
        //                    Run(0, 1);
        //                    x--;
        //                    Run(-1, 0);
        //                    Run(0, -1);
        //                    Run(0, -1);
        //                    Run(1, 0);
        //                }
        //                Run(0, 1);
        //                x--;
        //                Run(-1, 0);
        //                Run(0, -1);
        //            }
        //            while (y != emptyColor_Y + 1)
        //            {
        //                Run(1, 0);
        //                Run(0, 1);
        //                Run(-1, 0);
        //                Run(-1, 0);
        //                Run(0, -1);
        //                y--;
        //            }
        //            Run(1, 0);
        //            return;
        //        }
        //        if (x > emptyColor_X)
        //        {
        //            for (int i = emptyColor_X; i < x; i++) Run(0, 1);

        //            for (int i = emptyColor_Y; i < y; i++) Run(1, 0);

        //            y--;
        //            while (y != emptyColor_Y)
        //            {
        //                Run(0, -1);
        //                Run(-1, 0);
        //                Run(-1, 0);
        //                Run(0, 1);
        //                Run(1, 0);
        //                y--;
        //            }
        //            while (x != emptyColor_X)
        //            {
        //                Run(0, -1);
        //                Run(-1, 0);
        //                Run(0, 1);
        //                Run(1, 0);
        //                Run(0, -1);
        //                x--;
        //            }
        //            return;
        //        }
        //        if (y != emptyColor_Y + 1)
        //        {
        //            for (int i = emptyColor_Y; i < y - 1; i++) Run(1, 0);

        //            if (x == emptyColor_X)
        //            {
        //                Run(0, -1);
        //                Run(1, 0);
        //                Run(0, 1);
        //                Run(-1, 0);
        //                Run(0, -1);
        //                x--;
        //            }
        //            else for (int i = emptyColor_X; i > x; i--) Run(0, -1);

        //            while (y != emptyColor_Y + 2)
        //            {
        //                Run(1, 0);
        //                Run(0, 1);
        //                Run(-1, 0);
        //                Run(-1, 0);
        //                Run(0, -1);
        //                y--;
        //            }
        //            Run(1, 0);
        //            y--;
        //            Run(0, 1);
        //            Run(-1, 0);

        //            for (int i = x + 1; i < emptyColor_X; i++) Run(0, 1);

        //            Run(-1, 0);
        //        }

        //        for (int i = emptyColor_X; i > x; i--) Run(0, -1);

        //        Run(1, 0);
        //    }
        #endregion

        public Vector2 GetStep(Dictionary<Vector2, int> puzzle, Vector2 empty)
        {
            var arraySize = (int)Mathf.Sqrt(puzzle.Count);

            var dic = Vector2.zero;

            int g = 0;
            int f = Calculate_H(puzzle);
            Goal(puzzle, empty, empty, ref g);

            return dic;
        }

        private void Goal(Dictionary<Vector2, int> puzzle, Vector2 emptyPos, Vector2 oldEmptyPos, ref int g)
        {
            var tmp = new List<Vector2>();
            var directions = new List<Vector2> { Vector2.left, Vector2.down, Vector2.right, Vector2.up };
            for (var step = 0; step < 4; step++)
                if ((emptyPos + directions[step]) is Vector2 movePos && puzzle.TryGetValue(movePos, out _) && movePos != oldEmptyPos)
                    tmp.Add(directions[step]);

            var gg = ++g;
            int f = int.MaxValue, h = int.MaxValue;
            tmp.ForEach(dir =>
            {
                var hamming = Get_H(puzzle, dir, emptyPos, out Dictionary<Vector2, int> clonePuzzle);
                var manhattan = hamming + gg;

                if (manhattan < f || hamming < h)
                {
                    f = manhattan;
                    h = hamming;
                    puzzle = clonePuzzle;
                }
            });

            oldEmptyPos = emptyPos;
            emptyPos = FindPosition(puzzle, 0);

            Goal(puzzle, emptyPos, oldEmptyPos, ref g);
        }

        private int Get_H(Dictionary<Vector2, int> puzzle, Vector2 direction, Vector2 emptyPos, out Dictionary<Vector2, int> changedPuzzle)
        {
            changedPuzzle = puzzle.ToDictionary(entry => entry.Key, entry => entry.Value);
            var movePos = emptyPos + direction;

            if (!changedPuzzle.TryGetValue(movePos, out _)) return int.MaxValue;

            Swap(changedPuzzle, emptyPos, movePos);

            return Calculate_H(changedPuzzle);
        }

        private int Calculate_H(Dictionary<Vector2, int> puzzle)
        {
            var arraySize = (int)Mathf.Sqrt(puzzle.Count);
            var h = 0;
            for (int i = 0; i < puzzle.Count - 1; i++)
            {
                var dist = i % arraySize * Vector2.right + i / arraySize * Vector2.up - FindPosition(puzzle, i + 1);
                h += (int) (Mathf.Abs(dist.x) + Mathf.Abs(dist.y));
            }
            return h;
        }

        private bool IsSolved(Dictionary<Vector2, int> puzzle)
        {
            var arraySize = (int)Mathf.Sqrt(puzzle.Count);
            for (int i = 1; i < puzzle.Count; i++)
            {
                var old = puzzle[new Vector2((i - 1) % arraySize, (i - 1) / arraySize)];
                var current = puzzle[new Vector2(i % arraySize, i / arraySize)];
                if (old < current) continue;
                else return false;
            }
            return true;
        }

        private void Swap(Dictionary<Vector2, int> puzzle, Vector2 from, Vector2 to)
        {
            var temp = puzzle[to];
            puzzle[to] = puzzle[from];
            puzzle[from] = temp;
        }

        private Vector2 FindPosition(Dictionary<Vector2, int> puzzle, int value) =>
            Find(puzzle, item => item.Value == value).Key;

        private void ForEach<Tkey, Tvalue>(Dictionary<Tkey, Tvalue> dic, Action<KeyValuePair<Tkey, Tvalue>> action) {
            foreach (var entry in dic) action(entry);
        }

        private KeyValuePair<Tkey, Tvalue> Find<Tkey, Tvalue>(Dictionary<Tkey, Tvalue> dic, Predicate<KeyValuePair<Tkey, Tvalue>> action) {
            foreach (var entry in dic) if(action(entry)) return entry;
            return default;
        }

        private Vector2 GetSmall(Vector2 vector) => vector.x < vector.y ? Vector2.right * vector : Vector2.up * vector;
    }

    public struct Step
    {
        public readonly Vector2 Empty;
        public readonly Vector2 Change;

        public Step(Vector2 empty, Vector2 change)
        {
            Empty = new Vector2(empty.y - 1, empty.x - 1);
            Change = new Vector2(change.y - 1, change.x - 1);
        }
    }
}
