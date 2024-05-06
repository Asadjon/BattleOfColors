using System;
using System.Collections.Generic;
using static Assets.Scripts.GameOptions;

namespace Assets.Scripts.Records
{
    [Serializable]
    class LevelsOfRecordData : Dictionary<GameLevels, RecordData>
    {
        public LevelsOfRecordData() : base()
        {
            foreach (GameLevels gameLevels in Enum.GetValues(typeof(GameLevels)))
                Add(gameLevels, new RecordData());
        }

        public static implicit operator bool(LevelsOfRecordData typesOfRecordData) => typesOfRecordData != null;
    }
}
