using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Assets.Scripts.GameOptions;

namespace Assets.Scripts
{
    static class RecordHelper
    {
        private static readonly string fileName = "Records.json";
        private static string filePath = "/";
        private static Records Records = new Records();

        public static void Initialize(string filePath)
        {
            var record1 = new List<Record>();
            for (var i = MinNumberOfArrays; i <= MaxNumberOfArrays; i++)
                record1.Add(new Record() { numberOfArrays = i });

            var record2 = new List<Record>();
            for (var i = MinNumberOfArrays; i <= MaxNumberOfArrays; i++)
                record2.Add(new Record() { numberOfArrays = i });

            JsonFileReader.CreateFile(RecordHelper.filePath = filePath, fileName, new Records { recordsWithNumber = record1, recordsWithColor = record2 });
            Records = JsonFileReader.Read<Records>(filePath, fileName);
        }

        public static void SaveRecord(int numberOfArrays, GameTime recordTime, GameTypes isWithNumber)
        {
            var records = isWithNumber == GameTypes.WithNumber ? Records.recordsWithNumber : Records.recordsWithColor;

            var record = records.FirstOrDefault(r => r.numberOfArrays == numberOfArrays);
            record.time = recordTime;

            JsonFileReader.Write(Records, filePath, fileName);
        }

        public static Record GetRecord(int numberOfArrays, GameTypes isWithNumber) =>
            (isWithNumber == GameTypes.WithNumber ? Records.recordsWithNumber : Records.recordsWithColor).FirstOrDefault(r => r.numberOfArrays == numberOfArrays);
    }

    [Serializable] class Records { public List<Record> recordsWithNumber; public List<Record> recordsWithColor; }

    [Serializable] class Record
    {
        public GameTime time;
        public int numberOfArrays = DefaultNumberOfArrays;

        public Record() { }

        public Record(GameTime time, int numberOfArrays)
        {
            this.time = time;
            this.numberOfArrays = Mathf.Clamp(numberOfArrays, MinNumberOfArrays, MaxNumberOfArrays);
        }
    }

    [Serializable] struct GameTime
    {
        public int hour;
        public int minute;
        public int second;

        public static implicit operator TimeSpan(GameTime time) 
            => new TimeSpan(time.hour, time.minute, time.second);
    }
}
