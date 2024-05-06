using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets.Scripts
{
    public static class ScriptableObjectExtensions
    {
        public static List<T> GetObjectsAtPath<T>(this string path) where T : Object
        {
            var objs = new List<T>();

#if UNITY_EDITOR
            var fileEntries = Directory.GetFiles(Application.dataPath + "/" + path);

            foreach (var fileName in fileEntries)
            {
                var assetPathIndex = fileName.IndexOf("Assets");
                var localPath = fileName.Substring(assetPathIndex);

                if (AssetDatabase.LoadAssetAtPath<T>(localPath) is T obj && obj)
                    objs.Add(obj);
            }
#endif

            return objs;
        }
    }
}
