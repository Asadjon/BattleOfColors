using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Assets.Scripts.SaveGameDatas.Attributes
{
    internal static class SerializationController
    {
        public static object GetSavedValue(this object serializationObject) =>
            serializationObject.GetValueOfSerialization(serializationObject.GetType());

        public static void SetSavedValue(this object serializationObject, object serializedObject) =>
            serializedObject.SetValueOfSerialization(serializationObject);

        private static object GetValueOfSerialization(this object serializationObject, Type serializationType)
        {
            if (!serializationType.IsDefined(typeof(SerializationAttribute))) return null;

            var serializedType = serializationType.GetCustomAttribute<SerializationAttribute>().SerializedType;
            var serializedObject = Activator.CreateInstance(serializedType);

            if (serializedObject == null || !serializedType.TryGetMembersInfo(MemberTypes.Field, out FieldInfo[] fields)) return null;
            var serializedFields = fields.Where(field => field.IsDefined(typeof(SerializedMemberAttribute))).ToArray();

            foreach (var field in serializedFields)
            {
                var value = field.GetFieldValue(serializationObject, serializationType);

                if (field.FieldType.IsAssignableFrom(value.GetType()))
                    field.SetValue(serializedObject, value);
            }

            return serializedObject;
        }


        private static object GetFieldValue(this FieldInfo serializedField, object serializationObject, Type serializationType)
        {
            var serializedProperty = serializedField.GetCustomAttribute<SerializedMemberAttribute>();
            var serializationFieldName = serializedProperty != null ? serializedProperty.SerializedPropertyName : serializedField.Name;
            if (!serializationType.TryGetMemberInfo(serializationFieldName, out MemberInfo serializationMember)) return null;

            object serializationMemberValue = null;
            Type serializationMemberType = null;

            if (serializationMember is FieldInfo serializationField)
            {
                serializationMemberValue = serializationField.GetValue(serializationObject);
                serializationMemberType = serializationMemberValue.GetType();
            }
            else if (serializationMember is PropertyInfo serializationProperty)
            {
                serializationMemberValue = serializationProperty.GetValue(serializationObject);
                serializationMemberType = serializationMemberValue.GetType();
            }

            if (serializationMemberType.IsSubclassOf(typeof(Array)) && serializationMemberType.GetElementType().IsDefined(typeof(SerializationAttribute)))
            {
                var elementType = serializationMemberType.GetElementType();
                var arrayValue = (Array)serializationMemberValue;
                var serializationArray = Array.CreateInstance(elementType.GetCustomAttribute<SerializationAttribute>().SerializedType, arrayValue.Length);

                for (int i = 0; i < arrayValue.Length; i++)
                    serializationArray.SetValue(GetValueOfSerialization(arrayValue.GetValue(i), elementType), i);

                return serializationArray;
            }
            else if (serializationMemberType != null && GetValueOfSerialization(serializationMemberValue, serializationMemberType) is object value && value != null)
                return value;

            return serializationMemberValue;
        }

        private static void SetValueOfSerialization(this object serializedObject, object serializationObject)
        {
            if (serializedObject == null || !serializedObject.GetType().TryGetMembersInfo(MemberTypes.Field, out FieldInfo[] fields)) return;
            var serializedFields = fields.Where(field => field.IsDefined(typeof(SerializedMemberAttribute))).ToArray();

            foreach (var field in serializedFields)
                field.SetFieldValue(serializedObject, serializationObject);
        }

        private static void SetFieldValue(this FieldInfo serializedField, object serializedObject, object serializationObject)
        {
            var serializationFieldName = serializedField.IsDefined(typeof(SerializedMemberAttribute)) ? serializedField.GetCustomAttribute<SerializedMemberAttribute>().SerializedPropertyName : serializedField.Name;
            if (!serializationObject.GetType().TryGetMemberInfo(serializationFieldName, out MemberInfo serializationMember)) return;

            var serializationMemberType = serializationMember is FieldInfo info1 ? info1.FieldType : ((PropertyInfo)serializationMember).PropertyType;

            var serializedFieldValue = serializedField.GetValue(serializedObject);

            if (serializationMemberType.IsSubclassOf(typeof(Array)) && serializationMemberType.GetElementType() is Type elementType && elementType.IsDefined(typeof(SerializationAttribute)))
            {
                var arrayValue = (Array)serializedFieldValue;
                var serializationArray = Array.CreateInstance(elementType, arrayValue.Length);

                for (int i = 0; i < arrayValue.Length; i++)
                {
                    var serializationValue = Activator.CreateInstance(elementType, true);
                    if (serializationValue == null) serializationValue = Activator.CreateInstance(elementType, false);
                    arrayValue.GetValue(i).SetValueOfSerialization(serializationValue);
                    serializationArray.SetValue(serializationValue, i);
                }

                serializedFieldValue = serializationArray;
            }
            else if (serializationMemberType.IsDefined(typeof(SerializationAttribute)))
            {
                serializedFieldValue.SetValueOfSerialization(serializationMember.GetValue(serializationObject));
                return;
            }

            serializationMember.SetValue(serializationObject, serializedFieldValue);
        }

        private static void SetValue(this MemberInfo memberInfo, object typeObject, object value)
        {
            if (memberInfo is FieldInfo serializationField)
                serializationField.SetValue(typeObject, value);

            else if (memberInfo is PropertyInfo serializationProperty)
                serializationProperty.SetValue(typeObject, value);
        }

        private static object GetValue(this MemberInfo memberInfo, object typeObject)
        {
            if (memberInfo is FieldInfo serializationField)
                return serializationField.GetValue(typeObject);

            else if (memberInfo is PropertyInfo serializationProperty)
                return serializationProperty.GetValue(typeObject);

            else return null;
        }

        private static bool TryGetMemberInfo(this Type type, string name, out MemberInfo memberInfo, BindingFlags bf = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            memberInfo = null;

            while (type != null && type != typeof(object))
            {
                if (type.GetMember(name, bf) is MemberInfo[] membersInfo && membersInfo.Length > 0)
                {
                    memberInfo = membersInfo[0];
                    break;
                }

                type = type.BaseType;
            }

            return memberInfo != null;
        }

        private static bool TryGetMembersInfo<T>(this Type type, MemberTypes memberTypes, out T[] outMembersInfo, BindingFlags bf = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) where T : MemberInfo
        {
            List<Type> parents = new List<Type>();
            do {
                parents.Add(type);
                type = type.BaseType;
            } while (type != null && type != typeof(object));
            parents.Reverse();

            var outElementType = typeof(T);
            outMembersInfo = (T[])Array.CreateInstance(outElementType, 0);

            foreach (var parent in parents)
                foreach (var member in parent.GetMembers(bf))
                    if (!Array.Exists(outMembersInfo, m => m.Name == member.Name) && member.MemberType == memberTypes)
                    {
                        Array.Resize(ref outMembersInfo, outMembersInfo.Length + 1);
                        outMembersInfo[outMembersInfo.Length - 1] = (T)member;
                    }

            return outMembersInfo.Length > 0;
        }
    }
}
