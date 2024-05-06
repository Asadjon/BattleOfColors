using System;
using System.Collections.Generic;
using static Assets.Scripts.ActivityManager;

namespace Assets.Scripts.Activitys
{
    public abstract class Activity : UILayout
    {
        protected override void Awake()
        {
            if (ActivityManager.Instance)
                ActivityManager.Instance.AddActivity(this);
            base.Awake();
        }

        protected override void OnDestroy()
        {
            if (ActivityManager.Instance)
                ActivityManager.Instance.RemoveActivity(this);
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
            ActivityManager.Instance.UnloadActivity(gameObject.scene);

        protected void StartActivity<T>() where T : Activity =>
            ActivityManager.Instance.LoadActivity<T>();

        protected void StartActivity(Type activityType) =>
            ActivityManager.Instance.LoadActivity(activityType);

        protected void StartActivity<T>(Bundle bundle) where T : Activity =>
            ActivityManager.Instance.LoadActivity<T>(bundle);

        public class Bundle : Dictionary<string, object> { }
    }
}
