using System;
using UnityEngine;

namespace Assets.Scripts
{
    [Serializable]
    internal struct SerializebleVector2
    {
        public int x;
        public int y;

        public static implicit operator SerializebleVector2(Vector2Int position) =>
            new SerializebleVector2 { x = position.x, y = position.y };

        public static explicit operator Vector2Int(SerializebleVector2 position) =>
            new Vector2Int { x = position.x, y = position.y };
    }
}
