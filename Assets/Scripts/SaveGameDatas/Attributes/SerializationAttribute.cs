using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.SaveGameDatas.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class SerializationAttribute : Attribute
    {
        public Type SerializedType;

        public SerializationAttribute() { }

        public SerializationAttribute(Type serializedType)
        {
            SerializedType = serializedType;
        }
    }
}
