using System;
using System.Collections.Generic;
using static Assets.Scripts.GameOptions;

namespace Assets.Scripts.Records
{
    [Serializable]
    class CollectionOfRecords : Dictionary<SizesOfSquare, LevelsOfRecordData>
    {

        public CollectionOfRecords() : base(Enum.GetValues(typeof(SizesOfSquare)).Length)
        {            
            foreach (SizesOfSquare sizeOfSquare in Enum.GetValues(typeof(SizesOfSquare)))
                Add(sizeOfSquare, new LevelsOfRecordData());
        }

        public static implicit operator bool(CollectionOfRecords collectionOfRecords) => collectionOfRecords != null;
    }
}
