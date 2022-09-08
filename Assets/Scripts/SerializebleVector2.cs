using System;
using UnityEngine;

namespace Assets.Scripts
{
    [Serializable]
    internal struct SerializebleVector2
    {
        public float x;
        public float y;

        public static implicit operator SerializebleVector2(Vector2 position) =>
            new SerializebleVector2 { x = position.x, y = position.y };

        public static explicit operator Vector2(SerializebleVector2 position) =>
            new Vector2 { x = position.x, y = position.y };
    }
}
