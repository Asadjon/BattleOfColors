using Assets.Scripts.SaveGameDatas.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.SaveGameDatas
{
    internal interface ISerialization<T>
    {
        T Serialize(T obj);

        void Deserialize(T obj);
    }
}
