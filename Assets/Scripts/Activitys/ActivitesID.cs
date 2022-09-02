using Assets.Scripts.Custom;

namespace Assets.Scripts.Activitys
{
    public class ActivitesID : Singltone<ActivitesID>
    {
        [UnityEngine.SerializeField] private SerializableDictionary<string, int> m_Activities;

        public int GetId(System.Type type, int defId = -1) =>
            m_Activities.TryGetValue(type.Name, out int id) ? id : defId;

        public int GetId<T>(int defId = -1) => GetId(typeof(T), defId);

        protected override void LoadData() { }
    }
}
