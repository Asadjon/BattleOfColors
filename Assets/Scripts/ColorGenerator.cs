using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    static class ColorGenerator
    {
        public static bool CheckTheColors(this Color color1, Color color2, float distance = .4f)
        {
            Color.RGBToHSV(color1, out float h1, out float s1, out float v1);
            Color.RGBToHSV(color2, out float h2, out float s2, out float v2);

            return Vector3.Distance(new Vector3(v1, h1, s1), new Vector3(v2, h2, s2)) > distance;
        }

        public static Vector3 DiscolorToHSV(this Color color)
        {
            var sum = 0f;
            for (int i = 0; i < 3; i++) sum += color[i];
            var normal = sum / 3f;

            Color.RGBToHSV(new Color(normal, normal, normal, 1f), out float h, out float s, out float v);

            return new Vector3(h, s, v);
        }
    }
}
