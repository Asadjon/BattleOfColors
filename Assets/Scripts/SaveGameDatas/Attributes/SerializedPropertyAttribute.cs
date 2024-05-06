using System;

namespace Assets.Scripts.SaveGameDatas.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal class SerializedMemberAttribute : Attribute
    {
        public string SerializedMemberName;

        public SerializedMemberAttribute() { }

        public SerializedMemberAttribute(string serializedPropertyName)
        {
            SerializedMemberName = serializedPropertyName;
        }
    }
}
