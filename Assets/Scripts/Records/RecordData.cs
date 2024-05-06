using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Assets.Scripts.Records
{
    [Serializable]
    public class RecordData : ICloneable
    {
        public enum Parametrs { Time = 1, Moves = 2, All = Time | Moves }
        public enum MinMax { Min = 1, Max = 2, All = Min | Max }

        [JsonProperty] private TimeSpan mRecordTime;
        [JsonProperty] private int mMovesCount;
        [JsonProperty] private readonly List<RecordData> mLastTopRecords = new List<RecordData>();

        internal TimeSpan RecordTime => mRecordTime;
        internal int MovesCount => mMovesCount;

        public RecordData() { }

        public bool TrySetData(TimeSpan recordTime, int movesCount)
        {
            var newRecordData = new RecordData { mRecordTime = recordTime, mMovesCount = movesCount };
            if (Sum != 0 && Sum <= newRecordData.Sum) return false;

            if (Sum != 0) mLastTopRecords.Add((RecordData)Clone());

            mRecordTime = recordTime;
            mMovesCount = movesCount;

            RecordController.Instance.SaveData();

            return true;
        }

        public object Clone()
        {
            return new RecordData
            {
                mMovesCount = mMovesCount,
                mRecordTime = mRecordTime
            };
        }

        internal decimal GetAverage(Parametrs parametr)
        {
            if ((Parametrs.All | parametr) != Parametrs.All) return 0;

            var lastTopRecords = new List<RecordData>(mLastTopRecords) { this };

            if (parametr == Parametrs.All)
                return lastTopRecords.Average(record => record.Sum);

            if (parametr == Parametrs.Time)
                return (decimal)lastTopRecords.Average(record => record.mRecordTime.TotalSeconds);

            else return (decimal)lastTopRecords.Average(record => record.MovesCount);
        }

        internal decimal GetMinMax(Parametrs parametr, MinMax minMax)
        {
            if ((Parametrs.All | parametr) != Parametrs.All) return 0;

            var lastTopRecords = new List<RecordData>(mLastTopRecords) { this };

            if (minMax == MinMax.All)
            {
                if (parametr == Parametrs.All)
                    return lastTopRecords.Sum(record => record.Sum);

                if (parametr == Parametrs.Time)
                    return (decimal)lastTopRecords.Sum(record => record.mRecordTime.TotalSeconds);

                else return lastTopRecords.Sum(record => record.MovesCount);
            }
            else if (minMax == MinMax.Min)
            {
                if (parametr == Parametrs.All)
                    return lastTopRecords.Min(record => record.Sum);

                if (parametr == Parametrs.Time)
                    return (decimal)lastTopRecords.Min(record => record.mRecordTime.TotalSeconds);

                else return lastTopRecords.Min(record => record.MovesCount);
            }
            else
            {
                if (parametr == Parametrs.All)
                    return lastTopRecords.Max(record => record.Sum);

                if (parametr == Parametrs.Time)
                    return (decimal)lastTopRecords.Max(record => record.mRecordTime.TotalSeconds);

                else return lastTopRecords.Max(record => record.MovesCount);
            }
        }

        internal decimal Sum => (decimal)(mRecordTime.TotalSeconds + mMovesCount);

        public static implicit operator bool(RecordData recordData) => recordData != null;
    }
}
