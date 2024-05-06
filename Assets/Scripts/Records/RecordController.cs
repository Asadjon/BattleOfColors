using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static Assets.Scripts.GameOptions;

namespace Assets.Scripts.Records
{
    [Serializable]
    public class RecordController : Singltone<RecordController>
    {
        [JsonProperty] private readonly Dictionary<GameTypes, CollectionOfRecords> mRecordsOfGameTypes;
        private static readonly string mFileName = "Records.json";
        private static string mFilePath = "/";

        public static RecordController GetSavedInstance(string filePath)
        {
            JsonFileReader.CreateFolder(mFilePath = filePath);

            if (!JsonFileReader.Read(mFilePath, mFileName, out RecordController instance))
                instance = new RecordController();

            return instance;
        }

        private RecordController()
        {

            mRecordsOfGameTypes = new Dictionary<GameTypes, CollectionOfRecords>
        {
            { GameTypes.WithColor, new CollectionOfRecords() },
            { GameTypes.WithNumber, new CollectionOfRecords() }
        };
        }

        internal CollectionOfRecords this[GameTypes gameTypes]
        {
            get => GetRecord(gameTypes);
            set => ChangeCollectionOfRecords(gameTypes, value);
        }

        internal RecordController ChangeCollectionOfRecords(GameTypes gameTypes, CollectionOfRecords CollectionOfRecords)
        {
            if (mRecordsOfGameTypes.ContainsKey(gameTypes) && CollectionOfRecords)
            {
                mRecordsOfGameTypes[gameTypes] = CollectionOfRecords;
                SaveData();
            }
            return this;
        }

        internal CollectionOfRecords GetRecord(GameTypes gameTypes) =>
            mRecordsOfGameTypes[gameTypes];

        internal void SaveData()
        {
            JsonFileReader.Write(this, mFilePath, mFileName);
            UnityEngine.Debug.Log("Record datas saved");
        }
    }
}
