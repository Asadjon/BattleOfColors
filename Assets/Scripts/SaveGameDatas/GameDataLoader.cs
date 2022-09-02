using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Assets.Scripts.SaveGameDatas
{
    public static class GameDataLoader
    {
        public static readonly string Path = MainClass.DatasPath + "/GameDatas/";

        public static void Initialize()
        {
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);
        }

        public static void SaveData<T> (this T data, string fileName)
        {
            var stream = new FileStream(Path + fileName, FileMode.Create);

            new BinaryFormatter().Serialize(stream, data);
            stream.Close();
        }

        public static T LoadData<T>(string fileName, T defaultValue = default)
        {
            if (File.Exists(Path + fileName))
            {
                var stream = new FileStream(Path + fileName, FileMode.Open);

                var data = (T)new BinaryFormatter().Deserialize(stream);
                stream.Close();
                return data;
            }
            return defaultValue;
        }

        public static void DeleteData(string fileName)
        {
            if (File.Exists(Path + fileName))
                File.Delete(Path + fileName);
        }
    }
}
