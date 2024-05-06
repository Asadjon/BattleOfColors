using System;
using UnityEngine;

namespace Assets.Scripts
{
    public abstract class Singltone<T> where T : class
    {
        private static T mInstance;

        public static T Instance
        {
            get
            {
                if (mInstance != null) return mInstance;

                try
                {
                    mInstance = (T) Activator.CreateInstance(typeof(T), true);
                } catch (Exception e)
                {
                    Debug.LogException(e);
                }

                return mInstance;
            }
        }

        public static T GetInstance() => Instance;

        public static T NewInstance(T instance)
        {
            if (mInstance == null)
                mInstance = instance;

            else Debug.Log(instance.GetType().Name + " is already initialized!");

            return mInstance;
        }
    }
}
