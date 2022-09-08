using System;
using UnityEngine;

namespace Assets.Scripts
{
    [Serializable]
    internal struct SerializebleVector2Int
    {
        public int x;
        public int y;

        public static implicit operator SerializebleVector2Int(Vector2Int position) =>
            new SerializebleVector2Int { x = position.x, y = position.y };

        public static explicit operator Vector2Int(SerializebleVector2Int position) =>
            new Vector2Int { x = position.x, y = position.y };
    }
}
