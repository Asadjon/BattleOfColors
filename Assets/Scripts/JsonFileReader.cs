using System.IO;
using UnityEngine;

namespace Assets.Scripts
{
    public static class JsonFileReader
    {
        public static void CreateFolder(string filePath)
        {
            if (!Directory.Exists(MainClass.DatasPath + filePath))
                Directory.CreateDirectory(MainClass.DatasPath + filePath);
        }

        public static bool Read<T>(string filePath, string fileName, out T outputObject)
        {
            var path = MainClass.DatasPath + filePath + fileName;

            outputObject = default;
            if (!File.Exists(path)) return false;

            outputObject = JsonUtility.FromJson<T>(File.ReadAllText(path));
            return true;
        }

        public static void Write<T>(T obj, string filePath, string fileName)
        {
            var path = MainClass.DatasPath + filePath + fileName;

            if (!File.Exists(path))
                new FileStream(path, FileMode.Create, FileAccess.ReadWrite).Close();

            File.WriteAllText(path, JsonUtility.ToJson(obj));
        }
    }
}
