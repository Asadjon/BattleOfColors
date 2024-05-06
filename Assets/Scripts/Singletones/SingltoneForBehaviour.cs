using UnityEngine;

namespace Assets.Scripts
{
    public abstract class SingltoneForBehaviour<T> : MonoBehaviour where T : Component
    {
        private static T mInstance;
        public static T Instance { get => mInstance ? mInstance : (mInstance = FindObjectOfType<T>()); private set => mInstance = value; }

        [System.Obsolete]
        private void Awake()
        {
            if (!mInstance)
            {
                if (Application.isPlaying) DontDestroyOnLoad(gameObject);
                mInstance = this as T;
                LoadData();
                print("Instanced " + typeof(T).Name + "!");
            }
            else if (mInstance != this)
            {
                print("More than on instance " + typeof(T).Name + " found!");
                Destroy(gameObject);
            }
        }

        protected abstract void LoadData();
    }
}