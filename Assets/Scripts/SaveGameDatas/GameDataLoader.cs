using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Assets.Scripts.SaveGameDatas
{
    public static class GameDataLoader
    {
        public static readonly string Path = MainClass.DatasPath + "/saved_game_datas/";

        public static void Initialize()
        {
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);
        }

        public static void SaveData<T> (this T data, string fileName)
        {
            var stream = new FileStream(Path + fileName, FileMode.OpenOrCreate);

            new BinaryFormatter().Serialize(stream, data);
            stream.Close();
        }

        public static bool LoadData<T>(string fileName, out T outputValue)
        {
            outputValue = default;
            if (!File.Exists(Path + fileName)) return false;

            var stream = File.OpenRead(Path + fileName);

            outputValue = (T)new BinaryFormatter().Deserialize(stream);
            stream.Close();
            return true;
        }

        public static void DeleteData(string fileName)
        {
            if (File.Exists(Path + fileName))
                File.Delete(Path + fileName);
        }
    }
}
