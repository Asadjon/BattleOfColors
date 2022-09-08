using System;
using UnityEngine;

namespace Assets.Scripts
{
    [Serializable]
    internal struct SerializableVector2Int
    {
        public int x;
        public int y;

        public static implicit operator SerializableVector2Int(Vector2Int position) =>
            new SerializableVector2Int { x = position.x, y = position.y };

        public static explicit operator Vector2Int(SerializableVector2Int position) =>
            new Vector2Int { x = position.x, y = position.y };
    }
}
