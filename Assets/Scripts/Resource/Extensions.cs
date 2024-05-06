using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Resource
{
    internal static class Extensions
    {
        public static List<ViewResource> GenerateResources(this int count)
        {
            var resources = new List<ViewResource>();

            for (var i = 0; i < count; i++)
            {
                returnRandomColor:
                var color = UnityEngine.Random.ColorHSV(0f, 1f, .25f, 1f, .25f, 1f);
                if (i > 0 && !resources.TrueForAll(res => res.Color.CheckTheColors(color)))
                    goto returnRandomColor;

                resources.Add(new ViewResource(i, color));
            }

            return resources;
        }

        public static List<ViewResource> CreateMultiple(this List<ViewResource> directoryResources)
        {
            var resources = new List<ViewResource>();
            var count = (int)Mathf.Pow(directoryResources.Count, 2);

            for (var i = 0; i < count; i++)
                resources.Add(new ViewResource(directoryResources[i % directoryResources.Count])
                    .Set(i + 1, (i / directoryResources.Count + 1).ToString()));

            return resources;
        }
    }
}
