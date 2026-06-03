using System;
using System.Collections.Generic;
using static Assets.Scripts.ActivityManager;

namespace Assets.Scripts.Activitys
{
    public abstract class Activity : UILayout
    {
        protected ActivityManager _activityManager;

        protected override void Awake()
        {
            _activityManager = ActivityManager.Instance;

            if (_activityManager)
                _activityManager.AddActivity(this);
            base.Awake();
        }

        protected override void OnDestroy()
        {
            if (_activityManager)
                _activityManager.RemoveActivity(this);
            base.OnDestroy();
        }

        public override void OnBackPressed() => Finish();

        public virtual void OnPlay()
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        public abstract void OnCreate(Bundle bundle);

        public virtual void OnPause()
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }

        public virtual void Finish() =>
            _activityManager.UnloadActivity(gameObject.scene);

        protected void StartActivity<T>() where T : Activity =>
            _activityManager.LoadActivity<T>();

        protected void StartActivity(Type activityType) =>
            _activityManager.LoadActivity(activityType);

        protected void StartActivity<T>(Bundle bundle) where T : Activity =>
            _activityManager.LoadActivity<T>(bundle);

        public class Bundle : Dictionary<string, object> { }
    }
}
