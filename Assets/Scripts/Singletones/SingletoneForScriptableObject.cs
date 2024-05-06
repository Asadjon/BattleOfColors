
using System;
using System.Linq;
using UnityEngine;
using Assets.Scripts.Custom;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets.Scripts.Singletones
{
    public abstract class SingletoneForScriptableObject<T> : CustomScriptableObject where T : SingletoneForScriptableObject<T>
    {
        static T mInstance = null;
        public static T Instance { 
            get 
            {
                if (!mInstance && InitInstance() is T instances)
                {
                    if (!instances) throw new Exception("Could not find any " + typeof(T).Name + " instance");

                    mInstance = instances;
                }

                return mInstance;
            } 
            private set => mInstance = value; 
        }

        private static T InitInstance()
        {
            if (Resources.FindObjectsOfTypeAll<T>() is T[] instances && instances.Length > 0)
            {
                var instance = instances.First();

                if (instances.Length > 1)
                {
                    Debug.Log("More than on instance " + typeof(T).Name + " found!");

                    for (var i = 1; i < instances.Length; i++)
                        RemoveExcessSingleton(instances[i]);
                }

                return instance;
            }

            return null;
        }

        private static void RemoveExcessSingleton(T singleton)
        {
            if (!singleton) return;

            DestroyImmediate(singleton);
            //AssetDatabase.RemoveObjectFromAsset(singleton);
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();
        }

        protected static void Create(string path = "Assets/Resources/Singletone.asset")
        {
            if (!mInstance)
            {
                var instance = InitInstance();

                if (instance) mInstance = instance;
                else
                {
                    mInstance = CreateInstance<T>();

#if UNITY_EDITOR
                    AssetDatabase.CreateAsset(mInstance, path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    EditorUtility.FocusProjectWindow();
#endif
                }
            }
        }
    }
}