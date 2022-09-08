using UnityEngine;

namespace Assets.Scripts
{
    public abstract class Singltone<T> : MonoBehaviour where T : Object
    {
        private static T mInstance;
        public static T Instance { get => mInstance ? mInstance : (mInstance = FindObjectOfType<T>()); private set => mInstance = value; }

        private void Awake()
        {
            if (mInstance == null)
            {
                mInstance = (T)(object)this;
                LoadData();
            }
            else if (mInstance != this)
                print("More than on instance " + typeof(T).Name + " found!");
        }

        protected abstract void LoadData();
    }
}