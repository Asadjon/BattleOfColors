using Assets.Scripts.Activitys;
using Assets.Scripts.AudioManagers;
using Assets.Scripts.Records;
using Assets.Scripts.SaveGameDatas;
using System;
using UnityEngine;
using static Assets.Scripts.Activitys.Activity;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class MainClass : SingltoneForBehaviour<MainClass>
    {
        public static string DatasPath { get; private set; }

        [SerializeField] private string m_RecordsFilePath = "/";
        [SerializeField] private AudioManagerSingleton m_AudioManagerSingleton = null;

        protected override void LoadData()
        {
            DatasPath =
#if UNITY_EDITOR 
                Application.dataPath
#elif UNITY_ANDROID
                "/data/data/com." + Application.companyName + "." + Application.productName
#endif
                + "/game_datas";
            RecordController.NewInstance(RecordController.GetSavedInstance(m_RecordsFilePath));
            GameDataLoader.Initialize();
            m_AudioManagerSingleton.Stop(null);
        }

        private void Start()
        {
            ActivityManager.Instance.LoadActivity(typeof(MenuActivity));

            Type savingGameActivity;
            if (SinglePlayerGameActivity.TryGetSavedData(out _))
                savingGameActivity = typeof(SinglePlayerGameActivity);

            else if (MultiplayerGameActivity.TryGetSavedData(out _))
                savingGameActivity = typeof(MultiplayerGameActivity);
            else return;

            ActivityManager.Instance.LoadActivity(typeof(OptionsActivity), new Bundle { { OptionsActivity.ACTIVITY_TYPE, savingGameActivity } });
            ActivityManager.Instance.LoadActivity(savingGameActivity);
        }
    }
}
