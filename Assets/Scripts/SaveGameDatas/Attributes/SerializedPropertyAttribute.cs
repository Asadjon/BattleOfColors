using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.SaveGameDatas.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class SerializedMemberAttribute : Attribute
    {
        public string SerializedPropertyName;

        public SerializedMemberAttribute(string serializedPropertyName)
        {
            SerializedPropertyName = serializedPropertyName;
        }
    }
}
