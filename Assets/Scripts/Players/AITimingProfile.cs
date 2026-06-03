using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using static Assets.Scripts.GameOptions;

namespace Assets.Scripts.Players
{
    internal sealed class AITimingProfile
    {
        private const string FileName = "AI_Timing_Profile.json";
        private const string DirectoryName = "game_datas";
        private const float Smoothing = .2f;

        private static AITimingProfile _instance;

        [JsonProperty] private readonly List<Entry> mEntries = new();

        internal static bool TryGetAverageMoveTime(GameTypes gameType, GameLevels gameLevel, SizesOfSquare size, out float moveTime)
        {
            var entry = Instance.GetEntry(gameType, gameLevel, size, false);
            moveTime = entry?.AverageMoveTime ?? 0f;
            return entry != null && entry.Samples > 0 && moveTime > 0f;
        }

        internal static void RegisterPlayerGame(GameTypes gameType, GameLevels gameLevel, SizesOfSquare size, TimeSpan gameTime, int movesCount)
        {
            if (movesCount <= 0 || gameTime.TotalSeconds <= 0)
                return;

            var currentMoveTime = (float)(gameTime.TotalSeconds / movesCount);
            var entry = Instance.GetEntry(gameType, gameLevel, size, true);

            if (entry.Samples <= 0 || entry.AverageMoveTime <= 0f)
                entry.AverageMoveTime = currentMoveTime;
            else
                entry.AverageMoveTime = entry.AverageMoveTime * (1f - Smoothing) + currentMoveTime * Smoothing;

            entry.Samples++;
            Save();
        }

        private static AITimingProfile Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                Directory.CreateDirectory(DirectoryPath);

                if (!File.Exists(ProfilePath))
                {
                    _instance = new AITimingProfile();
                    return _instance;
                }

                _instance = JsonConvert.DeserializeObject<AITimingProfile>(File.ReadAllText(ProfilePath));
                _instance ??= new AITimingProfile();

                return _instance;
            }
        }

        private static string DirectoryPath => Path.Combine(Application.persistentDataPath, DirectoryName);
        private static string ProfilePath => Path.Combine(DirectoryPath, FileName);

        private static void Save()
        {
            Directory.CreateDirectory(DirectoryPath);
            File.WriteAllText(ProfilePath, JsonConvert.SerializeObject(Instance));
        }

        private Entry GetEntry(GameTypes gameType, GameLevels gameLevel, SizesOfSquare size, bool create)
        {
            var entry = mEntries.FirstOrDefault(e =>
                e.GameType == gameType &&
                e.GameLevel == gameLevel &&
                e.Size == size);

            if (entry != null || !create)
                return entry;

            entry = new Entry
            {
                GameType = gameType,
                GameLevel = gameLevel,
                Size = size
            };
            mEntries.Add(entry);
            return entry;
        }

        [Serializable]
        private sealed class Entry
        {
            public GameTypes GameType;
            public GameLevels GameLevel;
            public SizesOfSquare Size;
            public float AverageMoveTime;
            public int Samples;
        }
    }
}
