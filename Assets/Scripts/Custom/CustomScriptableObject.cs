using System;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Custom
{
    public class CustomScriptableObject : ScriptableObject
    {
#if UNITY_EDITOR

        public T[] GetChilds<T>() where T : ScriptableObject
        {
            var path = AssetDatabase.GetAssetPath(this);
            var objs = AssetDatabase.LoadAllAssetsAtPath(path);
            return Array.ConvertAll(Array.FindAll(objs, obj => obj != this && obj is T), tObj => (T) tObj);
        }

#endif
    }
}