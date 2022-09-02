using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public abstract class Singltone<T> : MonoBehaviour where T : MonoBehaviour
    {
        #region Singltone
        private static T mInstance;
        public static T Instance { get => mInstance ? mInstance : (mInstance = FindObjectOfType<T>()); private set => mInstance = value; }

        private void Awake()
        {
            if (mInstance == null)
            {
                LoadData();
                mInstance = (T)(object)this;
            }
            else if (mInstance != this)
                Debug.Log("More than on instance " + typeof(T).Name + " found!");
        }
        #endregion

        protected abstract void LoadData();
    }
}