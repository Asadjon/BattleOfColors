using Assets.Scripts.Activitys;
using UnityEngine;
using static UnityEngine.Screen;

namespace Assets.Scripts
{
    class MenuActivity : Activity
    {
        public void PlayGame_OnClick(int sceneId)
        {
            OptionsActivity.ActivityType = sceneId;
            StartTransitionAnim(ActivitesID.Instance.GetId<OptionsActivity>());
        }

        public void OpenRecords_OnClick() =>
            StartTransitionAnim(ActivitesID.Instance.GetId<RecordsActivity>());

        public void PlaySound() =>
            AudioManager.Instance.Play("Standard button click");


        #region Activites actions

        public override void StartActivity() => orientation = ScreenOrientation.Portrait;

        public override void WaitActivity() => Finish();

        public override void OnBackPressed() => Application.Quit();
        #endregion
    }
}
