using Assets.Scripts.Activitys;
using Assets.Scripts.SaveGameDatas;
using UnityEngine;

namespace Assets.Scripts
{
    class MainClass : MonoBehaviour
    {
        public static readonly string LastLoadedSceneId = "LastLoadedSceneId.dat";
        public static string DatasPath { get; private set; }

        [SerializeField] private string m_RecordsFilePath = "/";

        private void Awake()
        {
            DatasPath =
#if UNITY_EDITOR 
                Application.dataPath
#elif UNITY_ANDROID
                "/data/data/com." + Application.companyName + "." + Application.productName
#endif
                + "/game_datas";
            ActivityManager.NewInstanse();
            RecordHelper.Initialize(m_RecordsFilePath);
            GameDataLoader.Initialize();
        }

        private void Start()
        {
            ActivityManager.GetActivityManager.Start();

            ActivityManager.GetActivityManager.LoadActivity(
                GameDataLoader.LoadData(LastLoadedSceneId, out int sceneId) ? sceneId : ActivitesID.Instance.GetId<MenuActivity>());
        }
    }
}
