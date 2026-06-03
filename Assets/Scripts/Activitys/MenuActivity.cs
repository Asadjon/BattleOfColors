using UnityEngine;
using static UnityEngine.Screen;

namespace Assets.Scripts.Activitys
{
    public class MenuActivity : Activity
    {
        public override void OnCreate(Bundle bundle) => orientation = ScreenOrientation.Portrait;

        public void PlayGame_OnClick(int sceneId) =>
            StartActivity<OptionsActivity>(new Bundle { { OptionsActivity.ACTIVITY_TYPE, ActivitesID.Instance.GetActivityType(sceneId) } });

        public void OpenRecords_OnClick() => StartActivity<RecordsActivity>();
    }
}
