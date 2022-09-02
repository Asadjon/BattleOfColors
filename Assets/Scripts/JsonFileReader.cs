using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public static class JsonFileReader
    {
        public static void CreateFile<T>(string filePath, string fileName, T obj = default)
        {
            if(!Directory.Exists(MainClass.DatasPath + filePath))
                Directory.CreateDirectory(MainClass.DatasPath + filePath);

            if (File.Exists(MainClass.DatasPath + filePath + fileName)) return;

            File.Create(MainClass.DatasPath + filePath + fileName).Close();
            Write(obj, filePath, fileName);
        }

        public static T Read<T>(string filePath, string fileName)
        {
            var text = File.ReadAllText(MainClass.DatasPath + filePath + fileName);
            return JsonUtility.FromJson<T>(text);
        }

        public static void Write<T>(T obj, string filePath, string fileName)
        {
            var text = JsonUtility.ToJson(obj);
            File.WriteAllText(MainClass.DatasPath + filePath + fileName, text);
        }
    }
}
